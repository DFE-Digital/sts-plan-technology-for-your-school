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

const BATCH_SIZE = 50;

function readInputRows() {
  return new Promise((resolve, reject) => {
    const rows = [];
    fs.createReadStream('data/inputs.csv')
      .pipe(
        csv({
          // Remove BOM from headers if present
          mapHeaders: ({ header }) => header.replace(/^\uFEFF/, ''),
        }),
      )
      .on('data', (row) => rows.push(row))
      .on('end', () => resolve(rows))
      .on('error', reject);
  });
}

async function fetchUser(userRow, token) {
  const { dfeSignInRef } = userRow;
  const headers = { Authorization: `Bearer ${token}` };

  try {
    const [userResponse, orgResponse] = await Promise.all([
      axios.get(`${process.env.API_URL}/users/${dfeSignInRef}/organisationservices`, { headers }),
      axios.get(`${process.env.API_URL}/users/${dfeSignInRef}/v2/organisations`, { headers }),
    ]);

    const row = {
      ...userRow,
      email: userResponse.data['email'],
      name: `${userResponse.data['givenName']} ${userResponse.data['familyName']}`,
    };
    let orgs = [];

    if (!orgResponse.data.length) {
      row.organisations = 'No organisations listed for user';
    } else {
      row.organisations = orgResponse.data
        .map(
          (org) =>
            `${org.name} (${org.id ?? 'ID unknown'}) - ${org.GIASProviderType ?? 'Type unknown'}`,
        )
        .join(', ');

      orgs = orgResponse.data.map((org) => ({
        id: org.id ?? 'Unknown',
        urn: org.urn ?? 'Unknown',
        uid: org.uid ?? 'Unknown',
        ukprn: org.ukprn ?? 'Unknown',
        category: org.name,
        giasProviderType: org.GIASProviderType ?? 'Unknown',
        providerTypeName: org.providerTypeName ?? 'Unknown',
        status: org.status?.name ?? 'Unknown',
      }));
    }

    console.log(`API call succeeded for "${dfeSignInRef}"`);
    return { row, orgs };
  } catch (error) {
    console.error(`API call failed for "${dfeSignInRef}":`, error.message);
    return { row: { ...userRow, email: `Error: ${error.message}` }, orgs: [] };
  }
}

async function processCsv() {
  const inputRows = await readInputRows();
  const results = [];
  const organisations = [];

  for (let i = 0; i < inputRows.length; i += BATCH_SIZE) {
    const batch = inputRows.slice(i, i + BATCH_SIZE);
    console.log();
    console.log(
      `Processing users ${i + 1}-${i + batch.length} of ${inputRows.length} (batch of ${batch.length})`,
    );

    // A fresh token per batch avoids expiry during long runs
    const token = generateJwt();
    const batchResults = await Promise.all(batch.map((userRow) => fetchUser(userRow, token)));

    for (const { row, orgs } of batchResults) {
      results.push(row);
      organisations.push(...orgs);
    }
  }

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
  console.log(`Organisations CSV file successfully processed and saved as ${outputsOrgsFileName}`);
}

processCsv().catch((error) => {
  console.error('Fatal error:', error);
  process.exit(1);
});
