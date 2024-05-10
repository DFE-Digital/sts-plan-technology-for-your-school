import ContentError from "#src/errors/content-error";
import fs from "fs";

class ErrorLogger {
  errors = [];

  addError({ id, contentType, message }) {
    const error = new ContentError({ id, contentType, message });

    this.errors.push(error);
  }

  writeErrorsToFile(filePath = "content-errors.md") {
    const groupedByContentType = this.groupBy(this.errors, "contentType");

    const errors = `# Content Errors: \n\n` + Object.entries(groupedByContentType)
      .map(([contentType, contentErrors]) => this.errorMessagesForContentType(contentType, contentErrors))
      .join("\n");


    fs.writeFileSync(filePath, errors);
  }

  errorMessagesForContentType(contentType, errors) {
    let errorString =
      `## ${contentType}:

| Id | Message |
| -- | ------- |
`;

    for (const error of errors) {
      errorString += `| ${error.id} | ${error.message} | \n`;
    }

    errorString += "\n\n";

    return errorString;
  }

  groupBy(xs, key) {
    return xs.reduce(function (rv, x) {
      (rv[x[key]] = rv[x[key]] || []).push(x);
      return rv;
    }, {});
  };

}

const errorLogger = new ErrorLogger();

export default errorLogger;