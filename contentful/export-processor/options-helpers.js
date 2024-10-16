/**
 * Get argument value from options or environment
 * @param {object} obj - Object containing arguments (e.g. parsed CLI arguments or process.env)
 * @param {string} key - The key of the option
 * @param {boolean} isBoolean - Whether the value should be treated as a boolean
 * @returns {*} Value based on the options or environment, or undefined if not found
 */
const getArgumentValue = (obj, key, isBoolean) => {
  if (!isBoolean) {
    return obj[key];
  }

  if (obj[key] === undefined || obj[key] === null) {
    return;
  }

  return obj[key] === 'true' || obj[key] === true;
};

/**
 * Get value from options or environment
 * @param {Object} options - Parsed CLI arguments
 * @param {string} optionsKey - Key in the options object
 * @param {string} envKey - Environment variable key
 * @param {*} defaultValue - Default value if not found
 * @param {boolean} isBoolean - Whether to treat the value as a boolean
 * @returns {*} The final value, either from options or environment
 */
const getValueFromOptionsOrEnv = (options, optionsKey, envKey, defaultValue, isBoolean) => {
  const optionsValue = getArgumentValue(options, optionsKey, isBoolean);

  if (optionsValue !== undefined) {
    return optionsValue;
  }

  const environmentVariableValue = getArgumentValue(process.env, envKey, isBoolean);

  if (environmentVariableValue !== undefined) {
    return environmentVariableValue;
  }

  return defaultValue;
};

export {
  getValueFromOptionsOrEnv,
  getArgumentValue
};