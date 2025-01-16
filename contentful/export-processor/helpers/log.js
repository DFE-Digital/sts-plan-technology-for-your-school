const defaultLogOptions = {
    addBlankLine: true,
    addSeperator: false,
    seperator: "============",
    level: "INFO",
};

/**
 * Logs message to console with formatting
 * @param {string} message Message to log
 * @param {{addBlankLine: boolean, addSeperator: boolean, seperator: string, level: "INFO" | "WARNING" | "ERROR"}} options
 */
const logMessage = (message, options = defaultLogOptions) => {
    const { addBlankLine, addSeperator, seperator, level } = {
        ...defaultLogOptions,
        ...options,
    };

    const logger = level == "ERROR" ? console.error : console.log;

    const formattedMessage = formatMessage(
        level ?? defaultLogOptions.level,
        message
    );
    logger(formattedMessage);

    if (addBlankLine) {
        console.log("");
    }

    if (addSeperator) {
        console.log(seperator);
        console.log("");
    }
};

/**
 *
 * @param {string} level
 * @param {string} message
 * @returns
 */
const formatMessage = (level, message) =>
    `[${getTimeString()}] ${level} - ${message}`;

const getTimeString = () => {
    var now = new Date();
    return `${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;
};

export { logMessage as log };
