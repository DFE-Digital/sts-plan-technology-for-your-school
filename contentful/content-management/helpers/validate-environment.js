module.exports = function validateEnvironment() {
	const allowedEnvironments = ['PlanTech_DevAndTest', 'PlanTech_StagingAndProduction'];
	const environment = process.env.ENVIRONMENT;
  
	if (!environment) {
	  throw new Error('ENVIRONMENT environment variable is not set.');
	}
  
	if (!allowedEnvironments.includes(environment)) {
	  throw new Error(`Invalid Contentful environment`);
	}
  
	return environment;
  }