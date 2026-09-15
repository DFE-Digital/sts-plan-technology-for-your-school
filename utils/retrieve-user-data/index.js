require('dotenv').config();
const axios = require('axios');
const csv = require('csv-parser');
const fs = require('fs');
const jwt = require('jsonwebtoken');

const { Parser } = require('json2csv');

function generateJwt() {
  const payload = {
    iss: process.env.CLIENT_ID,
    aud: 'signin.education.gov.uk',
  };

  return jwt.sign(payload, process.env.API_SECRET, {
    algorithm: 'HS256',
    expiresIn: '5m',
  });
}

async function processCsv() {
  const token = generateJwt();

  console.log('Using the following JWT for API calls:', token);
  const promises = [];
  const results = [];
  const organisations = [];

  fs.createReadStream('data/inputs.csv')
    .pipe(
      csv({
        // Remove BOM from headers if present
        mapHeaders: ({ header }) => header.replace(/^\uFEFF/, ''),
      }),
    )
    .on('data', (userRow) => {
      const { dfeSignInRef } = userRow;
      const userDataEndpoint = `${process.env.API_URL}/users/${dfeSignInRef}/organisationservices`;
      const orgDataEndpoint = `${process.env.API_URL}/users/${dfeSignInRef}/v2/organisations`;

      const promise = Promise.all([
        axios.get(userDataEndpoint, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }),
        axios.get(orgDataEndpoint, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }),
      ])
        .then(([userResponse, orgResponse]) => {
          userRow.email = userResponse.data['email'];
          userRow.name = `${userResponse.data['givenName']} ${userResponse.data['familyName']}`;

          if (!orgResponse.data.length) {
            userRow.organisations = 'No organisations listed for user';
          } else {
            const userOrgs = orgResponse.data.map(
              (org) =>
                `${org.name} (${org.id ?? 'ID unknown'}) - ${org.GIASProviderType ?? 'Type unknown'}`,
            );
            userRow.organisations = userOrgs.join(', ');

            const orgs = orgResponse.data.map((org) => ({
              id: org.id ?? 'Unknown',
              urn: org.urn ?? 'Unknown',
              uid: org.uid ?? 'Unknown',
              ukprn: org.ukprn ?? 'Unknown',
              category: org.name,
              giasProviderType: org.GIASProviderType ?? 'Unknown',
              providerTypeName: org.providerTypeName ?? 'Unknown',
              status: org.status?.name ?? 'Unknown',
            }));

            organisations.push(...orgs);
          }

          results.push(userRow);
          console.log(`API call succeeded for "${dfeSignInRef}"`);
        })
        .catch((error) => {
          console.error(`API call failed for "${dfeSignInRef}":`, error.message);
          userRow.email = `Error: ${error.message}`;
          results.push(userRow);
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
      const outputsUsersFileName = `outputs-users-${dateStamp}T${timeStamp}.csv`;

      fs.writeFileSync(`data/${outputsUsersFileName}`, csvOutput);
      console.log(`Users CSV file successfully processed and saved as ${outputsUsersFileName}`);

      // Distinct orgs
      const distinctOrgs = Array.from(new Map(organisations.map((org) => [org.id, org])).values());
      const orgsCsvOutput = parser.parse(distinctOrgs);

      const outputsOrgsFileName = `outputs-orgs-${dateStamp}T${timeStamp}.csv`;
      fs.writeFileSync(`data/${outputsOrgsFileName}`, orgsCsvOutput);
      console.log(
        `Organisations CSV file successfully processed and saved as ${outputsOrgsFileName}`,
      );
    });
}

processCsv();
