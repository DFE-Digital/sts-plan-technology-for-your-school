const path = require("path")
const fs = require("fs")

async function main() {
    const fileName = process.argv[2]
    const filePath = fileName && path.join(__dirname, 'changes', fileName)

    if (!fs.existsSync(filePath)) {
        console.error("Invalid filename provided")
        return
    }

    const changeFunction = require(filePath)
    await changeFunction();
}

main();
