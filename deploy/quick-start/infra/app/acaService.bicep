/** Inputs **/
param apiKeySecretName string
param applicationInsightsName string
param containerAppsEnvironmentName string
param envSettings array = []
param exists bool
param hasIngress bool = false
param identityName string
param imageName string
param keyvaultName string
param location string = resourceGroup().location
param name string
param secretSettings array = []
param serviceName string
param storageAccountName string
param tags object = {}

@secure()
param appDefinition object

/** Locals **/
var appSettingsArray = filter(array(appDefinition.settings), i => i.name != '')
var formattedAppName = replace(name, '-', '')
var truncatedAppName = substring(formattedAppName, 0, min(length(formattedAppName), 32))

var env = union(map(filter(appSettingsArray, i => i.?secret == null), i => {
  name: i.name
  value: i.value
}), envSettings)

var secretNames = [
  '${serviceName}-apikey'
  apiKeySecretName
]

var secrets = union(map(filter(appSettingsArray, i => i.?secret != null), i => {
  name: i.name
  value: i.value
  secretRef: i.?secretRef ?? take(replace(replace(toLower(i.name), '_', '-'), '.', '-'), 32)
}), secretSettings)

/** Data Sources **/
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppsEnvironmentName
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

/** Resources **/
resource app 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: truncatedAppName
  location: location
  tags: union(tags, {'azd-service-name':  serviceName })
  dependsOn: [ secretsAccessPolicy, appConfigReaderRole ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: hasIngress ? {
        external: true
        targetPort: 80
        transport: 'auto'
      } : null
      secrets: union([
      ],
      map(secrets, secret => {
        identity: identity.id
        keyVaultUrl: secret.value
        name: secret.secretRef
      }))
    }
    template: {
      containers: [
        {
          image: fetchLatestImage.outputs.?containers[?0].?image ?? imageName
          name: 'main'
          env: union([
            {
              name: 'AZURE_CLIENT_ID'
              value: identity.properties.clientId
            }
            {
              name: 'AZURE_TENANT_ID'
              value: identity.properties.tenantId
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
            {
              name: 'PORT'
              value: '80'
            }
          ],
          env,
          map(secrets, secret => {
            name: secret.name
            secretRef: secret.secretRef
          }))
          resources: {
            cpu: json('1.0')
            memory: '2.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
    workloadProfileName: 'Warm'
  }
}

resource appConfigReaderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(subscription().id, resourceGroup().id, identity.id, 'appConfigReaderRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

resource apiKey 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: uniqueString(subscription().id, resourceGroup().id, app.id, serviceName)
    }
  }
]

resource blobContribRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(subscription().id, resourceGroup().id, identity.id, storageAccount.id, 'Storage Blob Data Contributor')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    )
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
  }
}

resource eventGridContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(subscription().id, resourceGroup().id, identity.id, 'eventGridContributorRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '1e241071-0855-49ea-94dc-649edcd759de')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource secretsAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyvault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: identity.properties.principalId
        permissions: { secrets: [ 'get', 'list' ] }
        tenantId: subscription().tenantId
      }
    ]
  }
}

resource secretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(subscription().id, resourceGroup().id, identity.id, 'secretsUserRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

resource queueContribRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: resourceGroup()
  name: guid(subscription().id, resourceGroup().id, identity.id, storageAccount.id, 'Storage Queue Data Contributor')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
    )
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
  }
}

/** Nested Modules **/
module fetchLatestImage '../modules/fetch-container-image.bicep' = {
  name: '${name}-fetch-image'
  params: {
    exists: exists
    name: imageName
  }
}

output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output id string = app.id
output miPrincipalId string = identity.properties.principalId
output name string = app.name
output uri string = hasIngress ? 'https://${app.properties.configuration.ingress.fqdn}' : ''
