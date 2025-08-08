import sql from 'mssql';
import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(process.cwd(), '.env') });

const config: sql.config = {
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  server: process.env.DB_SERVER as string,
  database: process.env.DB_DATABASE,
  options: {
    encrypt: process.env.ENCRYPT === 'true', 
    trustServerCertificate: process.env.TRUST_SERVER_CERTIFICATE === 'true', 
  },
};

export async function clearTestEstablishmentData(establishmentRef: string) {
  try {
    await runStoredProc('deleteDataForEstablishment', {
      establishmentRef: establishmentRef,
    });
  }
  catch (exception) {
    console.error(exception);
  }
}

async function runStoredProc(procName: string, inputParams: { [key: string]: any }
) {
  const pool = await sql.connect(config);
  const request = pool.request();

  for (const key in inputParams) {
    request.input(key, inputParams[key]); 
  }

  const result = await request.execute(procName);
  await pool.close();
  return result;
}