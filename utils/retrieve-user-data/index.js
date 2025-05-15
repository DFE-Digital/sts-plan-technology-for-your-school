require('dotenv').config();
const fs = require('fs');
const csv = require('csv-parser');
const axios = require('axios');
const jwt = require('jsonwebtoken');
const { Parser } = require('json2csv');

function generateJwt() {
    const payload = {
        iss: process.env.CLIENT_ID,
        aud: 'signin.education.gov.uk'
    };

    return jwt.sign(payload, process.env.API_SECRET, {
        algorithm: 'HS256',
        expiresIn: '5m'
    });
}

async function processCsv() {
    const token = generateJwt();
    const results = [];
    const promises = [];

    fs.createReadStream('data/inputs.csv')
        .pipe(csv())
        .on('data', (row) => {
            const value = row['dfeSignInRef'];
            const promise = axios.get(`${process.env.API_URL}/users/${value}/organisationservices`, {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            })
                .then(response => {
                    const extracted = response.data['email'];
                    results.push({ original: value, result: extracted });
                })
                .catch(error => {
                    console.error(`API call failed for "${value}":`, error);
                });

            promises.push(promise);
        })
        .on('end', async () => {
            await Promise.all(promises);

            if (results.length === 0) {
                console.error('No data to write. Check your input or API responses.');
                return;
            }

            const parser = new Parser();
            const csvOutput = parser.parse(results);


            const now = new Date();
            const dateStamp = now.toISOString().slice(0, 10).replace(/-/g, '');
            const timeStamp = now.toTimeString().slice(0, 5).replace(/:/g, '');
            const outputFileName = `outputs-${dateStamp}T${timeStamp}.csv`;

            fs.writeFileSync(`data/${outputFileName}`, csvOutput);
            console.log(`CSV file successfully processed and saved as ${outputFileName}`);
        });
}

processCsv();

