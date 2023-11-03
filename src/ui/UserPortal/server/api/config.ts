// Node may try dns resolution with IPv6 first which breaks the azure app
// configuration service requests, so we need to force it use IPv4 instead.
import dns from 'node:dns';
dns.setDefaultResultOrder('ipv4first');

import { AppConfigurationClient } from '@azure/app-configuration';

const allowedKeys = [
	'FoundationaLLM:APIs:CoreAPI:APIUrl',
	'.appconfig.featureflag/FoundationaLLM-AllowAgentHint',
	'FoundationaLLM:Branding:AllowAgentSelection',
	'FoundationaLLM:Branding:KioskMode',
	'FoundationaLLM:Branding:PageTitle',
	'FoundationaLLM:Branding:LogoUrl',
	'FoundationaLLM:Branding:LogoText',
	'FoundationaLLM:Branding:BackgroundColor',
	'FoundationaLLM:Branding:PrimaryColor',
	'FoundationaLLM:Branding:SecondaryColor',
	'FoundationaLLM:Branding:AccentColor',
	'FoundationaLLM:Branding:PrimaryTextColor',
	'FoundationaLLM:Branding:SecondaryTextColor',
	'FoundationaLLM:Chat:Entra:ClientId',
	'FoundationaLLM:Chat:Entra:Instance',
	'FoundationaLLM:Chat:Entra:TenantId',
	'FoundationaLLM:Chat:Entra:Scopes',
	'FoundationaLLM:Chat:Entra:CallbackPath',
];

export default defineEventHandler(async (event) => {
	const query = getQuery(event);
	const key = query.key;

	if (!allowedKeys.includes(key)) {
		console.error(`Config value "${key}" is not allowed to be accessed, please add it to the list of allowed keys if required.`);
		setResponseStatus(event, 403, `Config value "${key}" is not an accessible key.`);
		return '403';
	}

	const config = useRuntimeConfig();
	if (!config.APP_CONFIG_ENDPOINT) {
		console.error('APP_CONFIG_ENDPOINT env not found. Please ensure it is set.');
		setResponseStatus(event, 500, `Configuration endpoint missing.`);
		return '500';
	}
	const appConfigClient = new AppConfigurationClient(config.APP_CONFIG_ENDPOINT);

	try {
		const setting = await appConfigClient.getConfigurationSetting({ key });
		return setting.value;
	} catch (error) {
		console.error(`Failed to load config value for "${key}", please ensure it exists and is the correct format.`);
		setResponseStatus(event, 404, `Config value "${key}" not found.`);
		return '404';
	}
});
