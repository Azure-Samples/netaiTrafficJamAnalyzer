@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param principalId string

param principalType string

@description('The custom subdomain name for the OpenAI Cognitive Services account.')
param customSubDomainName string
@description('The kind of Cognitive Services account to create.')
param name string

@allowed([ 'Enabled', 'Disabled' ])
param publicNetworkAccess string = 'Disabled'

param disableLocalAuth bool = true
param kind string = 'OpenAI'

param allowedIpRules array = []
param networkAcls object = empty(allowedIpRules) ? {
  defaultAction: 'Allow'
} : {
  ipRules: allowedIpRules
  defaultAction: 'Deny'
}
param sku object = {
  name: 'S0'
}

resource openai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: name
  location: location
  kind: kind
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      ipRules: allowedIpRules
      bypass: 'AzureServices'      
      defaultAction: 'Deny'
    }
    disableLocalAuth: disableLocalAuth
  }
  sku: sku
  tags: {
    'aspire-resource-name': 'openai'
  }
}

resource openai_CognitiveServicesOpenAIContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openai.id, principalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a001fd3d-188f-4b5d-821b-7da978bf7442'))
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a001fd3d-188f-4b5d-821b-7da978bf7442')
    principalType: principalType
  }
  scope: openai
}

resource chat 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: openai
  name: 'chat'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-05-13'
    }
    raiPolicyName: 'raiPolicyName'
  }
  sku: {
    name: 'GlobalStandard'
    capacity: 10
  }
}

output connectionString string = 'Endpoint=${openai.properties.endpoint}'
