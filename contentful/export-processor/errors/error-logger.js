import ContentError from "./content-error.js";
import fs from "fs";

/**
 * @typedef {{id: string, contentType: string, message: string}} Error
 */

class ErrorLogger {
    errors = [];
    outputDir = "";

    /**
     *
     * @param {Error} error
     */
    addError({ id, contentType, message }) {
        const error = new ContentError({ id, contentType, message });

        this.errors.push(error);

        console.error(`${contentType} error for content ${id}: ${message}`);
    }

    writeErrorsToFile(filePath = "content-errors.md") {
        console.log(`Writing errors to file ${filePath}`);

        const groupedByContentType = this.groupBy(this.errors, "contentType");

        const errors =
            `# Content Errors: \n\n` +
            Object.entries(groupedByContentType)
                .map(([contentType, contentErrors]) =>
                    this.errorMessagesForContentType(contentType, contentErrors)
                )
                .join("\n\n");

        fs.writeFileSync(this.outputDir + filePath, errors);
        console.log(`Wrote errors to file ${filePath}`);
    }

    /**
     *
     * @param {string} contentType
     * @param {Error[]} errors
     * @returns {string} Error messages for a particular content type formatted as string
     */
    errorMessagesForContentType(contentType, errors) {
        let errorString = `## ${contentType}:

| Id | Message |
| -- | ------- |
`;

        for (const error of errors) {
            errorString += `| ${error.id} | ${error.message} |\n`;
        }

        return errorString.trim();
    }

    /**
     * Groups an array of errors by a specified key
     * @param {Error[]} errors Array of errors to group
     * @param {string} key The property to group by
     * @returns {Object} Grouped errors object
     */
    groupBy(errors, key) {
        return errors.reduce(function (groups, error) {
            (groups[error[key]] = groups[error[key]] || []).push(error);
            return groups;
        }, {});
    }
}

const errorLogger = new ErrorLogger();

export default errorLogger;

export { ErrorLogger as ErrorLoggerClass };
