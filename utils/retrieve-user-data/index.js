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
            const userDataEndpoint = `${process.env.API_URL}/users/${value}/organisationservices`;
            const orgDataEndpoint = `${process.env.API_URL}/users/${value}/v2/organisations`;

            const promise = Promise.all([
                axios.get(userDataEndpoint, {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }),
                axios.get(orgDataEndpoint, {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            ])
                .then(([ userResponse, orgResponse ]) => {
                    row.email = userResponse.data['email'];
                    row.name = `${userResponse.data['givenName']} ${userResponse.data['familyName']}`;

                    if (!orgResponse.data.length) {
                        row.organisations = "No organisations listed for user";
                    } else {
                        const orgs = orgResponse.data.map(org => `${org.name} - ${org.GIASProviderType ?? 'Type unknown'}`)
                        row.organisations = orgs.join(', ');
                    }

                    results.push(row);
                })
                .catch(error => {
                    console.error(`API call failed for "${value}":`, error.message);
                    row.email = `Error: ${error.message}`;
                    results.push(row);
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
