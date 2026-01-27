import sql from 'mssql';
import { DefaultAzureCredential } from '@azure/identity';
import 'dotenv/config';

export async function clearTestEstablishmentData(establishmentRef: string) {
  const pool = await connectToDatabase();

  await pool.connect();

  const request = pool.request();

  try {
    await createStoredProcRequest(request, {
      establishmentRef: establishmentRef,
    });

    const result = await request.execute('deleteDataForEstablishment');
    await pool.close();
    return result;
  } catch (exception) {
    console.error(exception);
  }
}

async function createStoredProcRequest(request: sql.Request, inputParams: { [key: string]: any }) {
  for (const key in inputParams) {
    request.input(key, inputParams[key]);
  }
}

async function connectToDatabase(): Promise<sql.ConnectionPool> {
  const connectionMode = process.env.DB_MODE;

  const pool: sql.ConnectionPool = await (connectionMode === 'azure'
    ? createAzureConnectionPool()
    : connectionMode === 'sql'
      ? createLocalSqlConnectionPool()
      : Promise.reject(
          new Error(
            "No connection mode set. DB_MODE in environment must be set to 'azure' or 'sql'.",
          ),
        ));

  return pool;
}

async function createAzureConnectionPool(): Promise<sql.ConnectionPool> {
  const server = process.env.DB_SERVER!;
  const database = process.env.DB_DATABASE!;

  // Get the access token from azure
  const credential = new DefaultAzureCredential();
  const token = await credential.getToken('https://database.windows.net/.default');

  // Connect using the AAD access token (no username/password)
  const pool = new sql.ConnectionPool({
    server,
    options: { encrypt: true, database, trustServerCertificate: false },
    authentication: {
      type: 'azure-active-directory-access-token',
      options: { token: token!.token },
    },
  });
  return pool;
}

async function createLocalSqlConnectionPool(): Promise<sql.ConnectionPool> {
  const config: sql.config = {
    user: process.env.DB_USER,
    password: process.env.DB_PASSWORD,
    server: process.env.DB_SERVER as string,
    database: process.env.DB_DATABASE,
    options: {
      encrypt: true,
      trustServerCertificate: true,
    },
  };

  const pool = sql.connect(config);
  return pool;
}
