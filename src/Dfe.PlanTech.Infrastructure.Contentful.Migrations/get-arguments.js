import minimist from "minimist";

function getArguments() {
    const expectedArguments = [{ arg: "se", name: "Source Environment" }, { arg: "te", name: "Target Environment" }, { arg: "space-id", name: "Space Id" }];

    const args = minimist(process.argv.slice(2));

    const errors = expectedArguments.map(argument => {
        const existing = args[argument.arg];

        if (!existing) {
            return `Missing ${argument.name} argument (--${argument.arg})`;
        }
    }).filter(error => error != undefined);

    if (errors.length > 0) {
        throw errors;
    }

    return args;
}

export default getArguments;