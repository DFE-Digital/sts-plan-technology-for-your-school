require('dotenv').config();
const axios = require('axios');
const jwt = require('jsonwebtoken');
const { sleepMs } = require('./utils');

const API_URL = process.env.API_URL;
const CLIENT_ID = process.env.CLIENT_ID;
const API_SECRET = process.env.API_SECRET;
const API_BACKOFF_MS = parseInt(process.env.API_BACKOFF_MS || '100', 10);

let tokenCache = { token: null, expiresAt: 0 };

function generateJwt() {
  const now = Date.now();
  if (tokenCache.token && tokenCache.expiresAt > now) return tokenCache.token;

  const token = jwt.sign({ iss: CLIENT_ID, aud: 'signin.education.gov.uk' }, API_SECRET, {
    algorithm: 'HS256',
    expiresIn: '5m',
  });

  tokenCache = { token, expiresAt: now + 4.5 * 60 * 1000 };
  return token;
}

function authHeader() {
  return { Authorization: `Bearer ${generateJwt()}` };
}

async function fetchUserOrgServices(userRef) {
  await sleepMs(API_BACKOFF_MS);
  const response = await axios.get(`${API_URL}/users/${userRef}/organisationservices`, {
    headers: authHeader(),
  });
  return response.data;
}

async function fetchUserOrgsV2(userRef) {
  await sleepMs(API_BACKOFF_MS);
  const response = await axios.get(`${API_URL}/users/${userRef}/v2/organisations`, {
    headers: authHeader(),
  });
  return response.data;
}

// Returns { users, numberOfRecords, page, numberOfPages }
async function fetchServiceUsersPage(page = 1, pageSize = 100) {
  await sleepMs(API_BACKOFF_MS);
  const response = await axios.get(`${API_URL}/users?page=${page}&pageSize=${pageSize}`, {
    headers: authHeader(),
  });
  return response.data;
}

// The GET /users paginated endpoint returns orgs with a different field shape than
// GET /users/{id}/v2/organisations. This normalises it to the shape upsertOrganisation expects.
function normalizeDiscoveredOrg(rawOrg) {
  return {
    id: rawOrg.id,
    name: rawOrg.name,
    category: rawOrg.Category ? { id: rawOrg.Category, name: null } : null,
    urn: rawOrg.URN,
    uid: rawOrg.UID,
    ukprn: rawOrg.UKPRN,
    upin: rawOrg.UPIN,
    establishmentNumber: rawOrg.EstablishmentNumber,
    status: rawOrg.Status ? { id: rawOrg.Status, name: null } : null,
    closedOn: rawOrg.ClosedOn,
    address: rawOrg.Address,
    telephone: rawOrg.telephone,
    statutoryLowAge: rawOrg.statutoryLowAge,
    statutoryHighAge: rawOrg.statutoryHighAge,
    legacyId: rawOrg.legacyId,
    companyRegistrationNumber: rawOrg.companyRegistrationNumber,
    ProviderProfileID: rawOrg.ProviderProfileID,
    ProviderTypeName: rawOrg.ProviderTypeName,
    LegalName: null,
    SourceSystem: rawOrg.SourceSystem,
    GIASProviderType: rawOrg.GIASProviderType,
    PIMSProviderType: rawOrg.PIMSProviderType,
    PIMSProviderTypeCode:
      rawOrg.PIMSProviderTypeCode != null ? parseInt(rawOrg.PIMSProviderTypeCode) : null,
    PIMSStatus: rawOrg.PIMSStatus,
    PIMSStatusName: rawOrg.PIMSStatusName,
    GIASStatus: null,
    GIASStatusName: null,
    MasterProviderStatusCode: null,
    MasterProviderStatusName: null,
    OpenedOn: rawOrg.OpenedOn,
    DistrictAdministrativeCode: null,
    DistrictAdministrativeName: rawOrg.DistrictAdministrativeName,
  };
}

module.exports = {
  fetchUserOrgServices,
  fetchUserOrgsV2,
  fetchServiceUsersPage,
  normalizeDiscoveredOrg,
};
