<template>
	<div>
		<h2 class="page-header">{{ editAgent ? 'Edit Agent' : 'Create New Agent' }}</h2>
		<div class="page-subheader">
			{{
				editAgent
					? 'Edit your agent settings below.'
					: 'Complete the settings below to create and deploy your new agent.'
			}}
		</div>

		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="span-2">
				<div class="step-header mb-2">Agent name:</div>
				<div class="mb-2">
					No special characters or spaces, lowercase letters with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="agentName"
						:disabled="editAgent"
						placeholder="Enter agent name"
						type="text"
						class="w-100"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						class="icon invalid"
						:title="validationMessage"
					>
						❌
					</span>
				</div>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Description:</div>
				<div class="mb-2">Provide a description to help others understand the agent's purpose.</div>
				<InputText
					v-model="agentDescription"
					placeholder="Enter agent description"
					type="text"
					class="w-100"
				/>
			</div>

			<!-- Type -->
			<div class="step-section-header span-2">Type</div>

			<div class="step-header span-2">What type of agent?</div>

			<!-- Knowledge management agent -->
			<div class="step">
				<div
					class="step-container cursor-pointer"
					@click="handleAgentTypeSelect('knowledge-management')"
				>
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton v-model="agentType" name="agentType" value="knowledge-management" />
							<div class="step-container__header">Knowledge Management</div>
						</div>
						<div>Best for Q&A, summarization and reasoning over textual data.</div>
					</div>
				</div>
			</div>

			<!-- Analytics agent-->
			<div class="step step--disabled">
				<div class="step-container cursor-pointer" @click="handleAgentTypeSelect('analytics')">
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton v-model="agentType" name="agentType" value="analytics" />
							<div class="step-container__header">Analytics</div>
						</div>
						<div>Best to query, analyze, calculate and report on tabular data.</div>
					</div>
				</div>
			</div>

			<!-- Knowledge source -->
			<div class="step-section-header span-2">Knowledge Source</div>

			<div class="step-header span-2">Do you want this agent to have a dedicated pipeline?</div>

			<div class="span-2">
				<div class="d-flex align-center mt-2">
					<span>
						<ToggleButton
							v-model="dedicated_pipeline"
							on-label="Yes"
							on-icon="pi pi-check-circle"
							off-label="No"
							off-icon="pi pi-times-circle"
						/>
					</span>
				</div>
			</div>

			<div v-if="dedicated_pipeline">
				<div class="step-header">Where is the data?</div>
			</div>
			<div class="step-header">Where should the data be indexed?</div>
			<div v-if="!dedicated_pipeline">
				<div class="step-header">How should the data be processed for indexing?</div>
			</div>

			<!-- Data source -->
			<div v-if="dedicated_pipeline">
				<CreateAgentStepItem v-model="editDataSource">
					<template v-if="selectedDataSource">
						<div class="step-container__header">{{ selectedDataSource.type }}</div>
						<div>
							<div v-if="selectedDataSource.object_id !== ''">
								<span class="step-option__header">Name:</span>
							</div>
							<span>{{ selectedDataSource.name }}</span>
						</div>
						<!-- <div>
							<span class="step-option__header">Container name:</span>
							<span>{{ selectedDataSource.Container.Name }}</span>
						</div> -->

						<!-- <div>
							<span class="step-option__header">Data Format(s):</span>
							<span v-for="format in selectedDataSource.Formats" :key="format" class="mr-1">
								{{ format }}
							</span>
						</div> -->
					</template>
					<template v-else>Please select a data source.</template>

					<template #edit>
						<div class="step-container__edit__header">Please select a data source.</div>

						<div v-for="(group, type) in groupedDataSources" :key="type">
							<div class="step-container__edit__group-header">{{ type }}</div>

							<div
								v-for="dataSource in group"
								:key="dataSource.name"
								class="step-container__edit__option"
								:class="{
									'step-container__edit__option--selected':
										dataSource.name === selectedDataSource?.name,
								}"
								@click.stop="handleDataSourceSelected(dataSource)"
							>
								<div>
									<div v-if="dataSource.object_id !== ''">
										<span class="step-option__header">Name:</span>
									</div>
									<span>{{ dataSource.name }}</span>
								</div>
								<!-- <div>
									<span class="step-option__header">Container name:</span>
									<span>{{ dataSource.Container.Name }}</span>
								</div> -->

								<!-- <div>
									<span class="step-option__header">Data Format(s):</span>
									<span v-for="format in dataSource.Formats" :key="format" class="mr-1">
										{{ format }}
									</span>
								</div> -->
							</div>
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<!-- Index source -->
			<CreateAgentStepItem v-model="editIndexSource">
				<template v-if="selectedIndexSource">
					<div v-if="selectedIndexSource.object_id !== ''">
						<div class="step-container__header">{{ selectedIndexSource.name }}</div>
						<div>
							<span class="step-option__header">URL:</span>
							<span>{{ selectedIndexSource.configuration_references.Endpoint }}</span>
						</div>
						<div>
							<span class="step-option__header">Index Name:</span>
							<span>{{ selectedIndexSource.settings.IndexName }}</span>
						</div>
					</div>
					<div v-else>
						<div class="step-container__header">DEFAULT</div>
						{{ selectedIndexSource.name }}
					</div>
				</template>
				<template v-else>Please select an index source.</template>

				<template #edit>
					<div class="step-container__edit__header">Please select an index source.</div>
					<div
						v-for="indexSource in indexSources"
						:key="indexSource.name"
						class="step-container__edit__option"
						:class="{
							'step-container__edit__option--selected':
								indexSource.name === selectedIndexSource?.name,
						}"
						@click.stop="handleIndexSourceSelected(indexSource)"
					>
						<div v-if="indexSource.object_id !== ''">
							<div class="step-container__header">{{ indexSource.name }}</div>
							<div v-if="indexSource.configuration_references.Endpoint">
								<span class="step-option__header">URL:</span>
								<span>{{ indexSource.configuration_references.Endpoint }}</span>
							</div>
							<div v-if="indexSource.settings.IndexName">
								<span class="step-option__header">Index Name:</span>
								<span>{{ indexSource.settings.IndexName }}</span>
							</div>
						</div>
						<div v-else>
							<div class="step-container__header">DEFAULT</div>
							{{ indexSource.name }}
						</div>
					</div>
				</template>
			</CreateAgentStepItem>

			<div v-if="dedicated_pipeline">
				<div class="step-header">How should the data be processed for indexing?</div>
			</div>
			<div v-if="dedicated_pipeline">
				<div class="step-header">When should the data be indexed?</div>
			</div>

			<!-- Process indexing -->
			<CreateAgentStepItem>
				<div class="step-container__header">Splitting & Chunking</div>

				<div>
					<span class="step-option__header">Chunk size:</span>
					<span>{{ chunkSize }}</span>
				</div>

				<div>
					<span class="step-option__header">Overlap size:</span>
					<span>{{ overlapSize == 0 ? 'No Overlap' : overlapSize }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Splitting & Chunking</div>

					<div>
						<span class="step-option__header">Chunk size:</span>
						<InputText v-model="chunkSize" type="number" class="mt-2" />
					</div>

					<div>
						<span class="step-option__header">Overlap size:</span>
						<InputText v-model="overlapSize" type="number" class="mt-2" />
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Trigger -->
			<div v-if="dedicated_pipeline">
				<CreateAgentStepItem>
					<div class="step-container__header">Trigger</div>
					<div>Runs every time a new item is added to the data source.</div>

					<div class="mt-2">
						<span class="step-option__header">Frequency:</span>
						<span>{{ triggerFrequency }}</span>
					</div>

					<div v-if="triggerFrequency === 'Schedule' && triggerFrequencyScheduled">
						<span class="step-option__header">Schedule:</span>
						<span>{{ triggerFrequencyScheduled }}</span>
					</div>

					<template #edit>
						<div class="step-container__header">Trigger</div>
						<div>Runs every time a new item is added to the data source.</div>

						<div class="mt-2">
							<span class="step-option__header">Frequency:</span>
							<Dropdown
								v-model="triggerFrequency"
								class="dropdown--agent"
								:options="triggerFrequencyOptions"
								placeholder="--Select--"
							/>
						</div>

						<div v-if="triggerFrequency === 'Schedule'" class="mt-2">
							<span class="step-option__header">Select schedule:</span>
							<Dropdown
								v-model="triggerFrequencyScheduled"
								class="dropdown--agent"
								:options="triggerFrequencyScheduledOptions"
								placeholder="--Select--"
							/>
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<!-- Agent configuration -->
			<div class="step-section-header span-2">Agent Configuration</div>

			<div class="step-header">Should conversations be saved?</div>
			<div class="step-header">How should user-agent interactions be gated?</div>

			<!-- Conversation history -->
			<CreateAgentStepItem>
				<div class="step-container__header">Conversation History</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ conversationHistory ? 'Yes' : 'No' }}</span>
						<span
							v-if="conversationHistory"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Max Messages:</span>
					<span>{{ conversationMaxMessages }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Conversation History</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="conversationHistory"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div>
						<span class="step-option__header">Max Messages:</span>
						<InputText v-model="conversationMaxMessages" type="number" class="mt-2" />
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Gatekeeper -->
			<CreateAgentStepItem>
				<div class="step-container__header">Gatekeeper</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ gatekeeperEnabled ? 'Yes' : 'No' }}</span>
						<span
							v-if="gatekeeperEnabled"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Content Safety:</span>
					<span>{{ gatekeeperContentSafety.label }}</span>
				</div>

				<div>
					<span class="step-option__header">Data Protection:</span>
					<span>{{ gatekeeperDataProtection.label }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Gatekeeper</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="gatekeeperEnabled"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Content Safety:</span>
						<Dropdown
							v-model="gatekeeperContentSafety"
							class="dropdown--agent"
							:options="gatekeeperContentSafetyOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Data Protection:</span>
						<!-- <span>Microsoft Presidio</span> -->
						<Dropdown
							v-model="gatekeeperDataProtection"
							class="dropdown--agent"
							:options="gatekeeperDataProtectionOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Orchestrator -->
			<div class="step-header span-2">Which orchestrator should the agent use?</div>
			<div class="span-2">
				<Dropdown
					v-model="orchestrator"
					:options="orchestratorOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="dropdown--agent"
				/>
			</div>

			<!-- System prompt -->
			<div class="step-section-header span-2">System Prompt</div>

			<div class="step-header">What is the persona of the agent?</div>

			<div class="span-2">
				<Textarea
					v-model="systemPrompt"
					class="w-100"
					auto-resize
					rows="5"
					type="text"
					placeholder="You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional."
				/>
			</div>

			<div class="button-container column-2 justify-self-end">
				<!-- Create agent -->
				<Button
					:label="editAgent ? 'Save Changes' : 'Create Agent'"
					severity="primary"
					@click="handleCreateAgent"
				/>

				<!-- Cancel -->
				<Button
					v-if="editAgent"
					style="margin-left: 16px"
					label="Cancel"
					severity="secondary"
					@click="handleCancel"
				/>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import type {
	Agent,
	AgentIndex,
	AgentDataSource,
	CreateAgentRequest,
	// AgentCheckNameResponse,
} from '@/js/types';

const defaultSystemPrompt: string = '';

const defaultFormValues = {
	agentName: '',
	agentDescription: '',
	object_id: '',
	text_partitioning_profile_object_id: '',
	text_embedding_profile_object_id: '',
	vectorization_data_pipeline_object_id: '',
	prompt_object_id: '',
	dedicated_pipeline: true,
	agentType: 'knowledge-management' as CreateAgentRequest['type'],

	editDataSource: false as boolean,
	selectedDataSource: null as null | AgentDataSource,

	editIndexSource: false as boolean,
	selectedIndexSource: null as null | AgentIndex,

	chunkSize: 500,
	overlapSize: 50,

	triggerFrequency: 'Event' as string,
	triggerFrequencyScheduled: '' as string,

	conversationHistory: false as boolean,
	conversationMaxMessages: 5 as number,

	gatekeeperEnabled: false as boolean,
	gatekeeperContentSafety: { label: 'None', value: null },
	gatekeeperDataProtection: { label: 'None', value: null },

	systemPrompt: defaultSystemPrompt as string,
	orchestrator: 'LangChain' as string,
};

export default {
	name: 'CreateAgent',

	props: {
		editAgent: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			...defaultFormValues,

			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: '' as string,

			dataSources: [] as AgentDataSource[],
			indexSources: [] as AgentIndex[],

			orchestratorOptions: [
				{
					label: 'LangChain',
					value: 'LangChain',
				},
			],

			triggerFrequencyOptions: ['Event', 'Manual'],

			triggerFrequencyScheduledOptions: [
				'Never',
				'Every 30 minutes',
				'Hourly',
				'Every 12 hours',
				'Daily',
			],

			gatekeeperContentSafetyOptions: [
				{
					label: 'None',
					value: null,
				},
				{
					label: 'Azure Content Safety',
					value: 'ContentSafety',
				},
			],

			gatekeeperDataProtectionOptions: [
				{
					label: 'None',
					value: null,
				},
				{
					label: 'Microsoft Presidio',
					value: 'Presidio',
				},
			],
		};
	},

	computed: {
		groupedDataSources() {
			const grouped = {};
			this.dataSources.forEach((dataSource) => {
				if (!grouped[dataSource.type]) {
					grouped[dataSource.type] = [];
				}

				grouped[dataSource.type].push(dataSource);
			});

			return grouped;
		},
	},

	async created() {
		this.loading = true;
		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		try {
			this.loadingStatusText = 'Retrieving indexes...';
			this.indexSources = await api.getAgentIndexes(true);

			this.loadingStatusText = 'Retrieving data sources...';
			this.dataSources = await api.getAgentDataSources(true);
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
			});
		}

		if (this.editAgent) {
			this.loadingStatusText = `Retrieving agent "${this.editAgent}"...`;
			const agent = await api.getAgent(this.editAgent);
			if (agent.vectorization && agent.vectorization.text_partitioning_profile_object_id) {
				this.loadingStatusText = `Retrieving text partitioning profile...`;
				const textPartitioningProfile = await api.getTextPartitioningProfile(
					agent.vectorization.text_partitioning_profile_object_id,
				);
				if (textPartitioningProfile) {
					this.chunkSize = Number(textPartitioningProfile.settings.ChunkSizeTokens);
					this.overlapSize = Number(textPartitioningProfile.settings.OverlapSizeTokens);
				}
			}
			if (agent.prompt_object_id !== '') {
				this.loadingStatusText = `Retrieving prompt...`;
				const prompt = await api.getPrompt(agent.prompt_object_id);
				if (prompt) {
					this.systemPrompt = prompt.prefix;
				}
			}
			this.loadingStatusText = `Mapping agent values to form...`;
			this.mapAgentToForm(agent);
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		mapAgentToForm(agent: Agent) {
			this.agentName = agent.name || this.agentName;
			this.agentDescription = agent.description || this.agentDescription;
			this.agentType = agent.type || this.agentType;
			this.object_id = agent.object_id || this.object_id;
			this.orchestrator = agent.orchestration_settings?.orchestrator || this.orchestrator;
			if (agent.vectorization) {
				this.dedicated_pipeline = agent.vectorization.dedicated_pipeline;
			}
			this.text_embedding_profile_object_id =
				agent.vectorization?.text_embedding_profile_object_id ||
				this.text_embedding_profile_object_id;

			this.triggerFrequency = agent.vectorization?.trigger_type || this.triggerFrequency;
			this.triggerFrequencyScheduled =
				agent.vectorization?.trigger_cron_schedule || this.triggerFrequencyScheduled;

			this.selectedIndexSource =
				this.indexSources.find(
					(indexSource) =>
						indexSource.object_id === agent.vectorization?.indexing_profile_object_id,
				) || null;

			this.selectedDataSource =
				this.dataSources.find(
					(dataSource) => dataSource.object_id === agent.vectorization?.data_source_object_id,
				) || null;

			this.conversationHistory = agent.conversation_history?.enabled || this.conversationHistory;
			this.conversationMaxMessages =
				agent.conversation_history?.max_history || this.conversationMaxMessages;

			this.gatekeeperEnabled = Boolean(agent.gatekeeper?.use_system_setting);

			this.gatekeeperContentSafety =
				this.gatekeeperContentSafetyOptions.find((localOption) =>
					agent.gatekeeper.options.find((option) => option === localOption.value),
				) || this.gatekeeperContentSafety;

			this.gatekeeperDataProtection =
				this.gatekeeperDataProtectionOptions.find((localOption) =>
					agent.gatekeeper.options.find((option) => option === localOption.value),
				) || this.gatekeeperDataProtection;
		},

		async checkName() {
			try {
				const response = await api.checkAgentName(this.agentName, this.agentType);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
					// this.$toast.add({
					// 	severity: 'warn',
					// 	detail: `Agent name "${this.agentName}" is already taken for the selected ${response.type} agent type. Please choose another name.`,
					// });
				}
			} catch (error) {
				console.error('Error checking agent name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the agent name. Please try again.';
			}
		},

		resetForm() {
			for (const key in defaultFormValues) {
				this[key] = defaultFormValues[key];
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}
			this.$router.push('/agents/public');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.agentName = sanitizedValue;

			// Check if the name is available if we are creating a new agent.
			if (!this.editAgent) {
				this.debouncedCheckName();
			}
		},

		handleAgentTypeSelect(type: Agent['type']) {
			this.agentType = type;
		},

		handleDataSourceSelected(dataSource: AgentDataSource) {
			this.selectedDataSource = dataSource;
			this.editDataSource = false;
		},

		handleIndexSourceSelected(indexSource: AgentIndex) {
			this.selectedIndexSource = indexSource;
			this.editIndexSource = false;
		},

		async handleCreateAgent() {
			const errors = [];
			if (!this.agentName) {
				errors.push('Please give the agent a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (this.text_embedding_profile_object_id === '') {
				const textEmbeddingProfiles = await api.getTextEmbeddingProfiles();
				if (textEmbeddingProfiles.length === 0) {
					errors.push('No vectorization text embedding profiles found.');
				} else {
					this.text_embedding_profile_object_id = textEmbeddingProfiles[0].object_id;
				}
			}

			// if (!this.selectedDataSource) {
			// 	errors.push('Please select a data source.');
			// }

			// if (!this.selectedIndexSource) {
			// 	errors.push('Please select an index source.');
			// }

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loading = true;
			this.loadingStatusText = 'Creating agent...';

			const promptRequest = {
				type: 'multipart',
				name: this.agentName,
				description: `System prompt for the ${this.agentName} agent`,
				prefix: this.systemPrompt,
				suffix: '',
			};

			const tokenTextPartitionRequest = {
				text_splitter: 'TokenTextSplitter',
				name: this.agentName,
				settings: {
					Tokenizer: 'MicrosoftBPETokenizer',
					TokenizerEncoder: 'cl100k_base',
					ChunkSizeTokens: this.chunkSize.toString(),
					OverlapSizeTokens: this.overlapSize.toString(),
				},
			};

			let successMessage = null;
			try {
				// Handle Prompt creation/update.
				let promptObjectId = '';
				if (promptRequest.prefix !== '') {
					const promptResponse = await api.createOrUpdatePrompt(this.agentName, promptRequest);
					promptObjectId = promptResponse.objectId;
				}

				// Handle TextPartitioningProfile creation/update.
				const tokenTextPartitionResponse = await api.createOrUpdateTextPartitioningProfile(
					this.agentName,
					tokenTextPartitionRequest,
				);
				const textPartitioningProfileObjectId = tokenTextPartitionResponse.objectId;

				// Select the default data source, if any.
				let dataSourceObjectId = this.selectedDataSource?.object_id ?? '';
				if (dataSourceObjectId === '') {
					const defaultDataSource = await api.getDefaultDataSource();
					if (defaultDataSource !== null) {
						dataSourceObjectId = defaultDataSource.object_id;
					}
				}

				// Select the default indexing profile, if any.
				let indexingProfileObjectId = this.selectedIndexSource?.object_id ?? '';
				if (indexingProfileObjectId === '') {
					const defaultAgentIndex = await api.getDefaultAgentIndex();
					if (defaultAgentIndex !== null) {
						indexingProfileObjectId = defaultAgentIndex.object_id;
					}
				}

				const agentRequest: CreateAgentRequest = {
					type: this.agentType,
					name: this.agentName,
					description: this.agentDescription,
					object_id: this.object_id,

					vectorization: {
						dedicated_pipeline: this.dedicated_pipeline,
						text_embedding_profile_object_id: this.text_embedding_profile_object_id,
						indexing_profile_object_id: indexingProfileObjectId,
						text_partitioning_profile_object_id: textPartitioningProfileObjectId,
						data_source_object_id: dataSourceObjectId,
						vectorization_data_pipeline_object_id: this.vectorization_data_pipeline_object_id,
						trigger_type: this.triggerFrequency,
						trigger_cron_schedule: '',
					},

					conversation_history: {
						enabled: this.conversationHistory,
						max_history: Number(this.conversationMaxMessages),
					},

					gatekeeper: {
						use_system_setting: this.gatekeeperEnabled,
						options: [
							this.gatekeeperContentSafety.value as unknown as string,
							this.gatekeeperDataProtection.value as unknown as string,
						].filter((option) => option !== null),
					},

					language_model: {
						type: 'openai',
						provider: 'microsoft',
						temperature: 0,
						use_chat: true,
						api_endpoint: 'FoundationaLLM:AzureOpenAI:API:Endpoint',
						api_key: 'FoundationaLLM:AzureOpenAI:API:Key',
						api_version: 'FoundationaLLM:AzureOpenAI:API:Version',
						version: 'FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion',
						deployment: 'FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName',
					},

					sessions_enabled: true,

					prompt_object_id: promptObjectId,
					orchestration_settings: {
						orchestrator: this.orchestrator,
						endpoint_configuration: {
							endpoint: 'FoundationaLLM:AzureOpenAI:API:Endpoint',
							api_key: 'FoundationaLLM:AzureOpenAI:API:Key',
						},
						model_parameters: {
							temperature: 0,
							deployment_name: 'FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName',
						},
					},
				};

				if (this.editAgent) {
					await api.updateAgent(this.editAgent, agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully updated!`;
				} else {
					await api.createAgent(agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully created!`;
					this.resetForm();
				}
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
			});

			this.loading = false;

			if (!this.editAgent) {
				this.$router.push('/agents/public');
			}
		},
	},
};
</script>

<style lang="scss">
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.steps--loading {
	pointer-events: none;
}

.steps__loading-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	z-index: 10;
	background-color: rgba(255, 255, 255, 0.9);
	pointer-events: none;
}

.step-section-header {
	background-color: rgba(150, 150, 150, 1);
	color: white;
	font-size: 1rem;
	font-weight: 600;
	padding: 16px;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.step {
	display: flex;
	flex-direction: column;
}

.step--disabled {
	pointer-events: none;
	opacity: 0.5;
}

.step-container {
	// padding: 16px;
	border: 2px solid #e1e1e1;
	flex-grow: 1;
	position: relative;

	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}

	&__header {
		font-weight: bold;
		margin-bottom: 8px;
	}
}

.step-container__view {
	// padding: 16px;
	height: 100%;
	display: flex;
	flex-direction: row;
}

.step-container__view__inner {
	padding: 16px;
	flex-grow: 1;
	word-break: break-word;
}

.step-container__view__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	width: 40px;
	min-width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;

	&:hover {
		background-color: #cacaca;
	}
}

$editStepPadding: 16px;
.step-container__edit {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	min-height: calc(100% + 4px);
	// padding: $editStepPadding;
	display: flex;
	flex-direction: row;
}

.step-container__edit__inner {
	padding: $editStepPadding;
	flex-grow: 1;
}

.step-container__edit__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	min-width: 40px;
	width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;
	transform: rotate(180deg);

	&:hover {
		background-color: #cacaca;
	}
}

.step-container__edit-dropdown {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	display: flex;
	flex-direction: column;
	min-height: calc(100% + 4px);
}

.step-container__edit__header {
	padding: $editStepPadding;
}

.step-container__edit__group-header {
	font-weight: bold;
	padding: $editStepPadding;
	padding-bottom: 0px;
}

.step-container__edit__option {
	padding: $editStepPadding;
	word-break: break-word;
	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}
}

// .step-container__edit__option + .step-container__edit__option{
// 	border-top: 2px solid #e1e1e1;
// }

.step-container__edit__option--selected {
	// outline: 2px solid #e1e1e1;
	// background-color: rgba(217, 217, 217, 0.4);
}

.step__radio {
	display: flex;
	gap: 10px;
}

.step-option__header {
	text-decoration: underline;
	margin-right: 8px;
}

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

input {
	width: 100%;
	padding-right: 30px;
}

.icon {
	position: absolute;
	right: 10px;
	cursor: default;
}

.valid {
	color: green;
}

.invalid {
	color: red;
}
</style>
