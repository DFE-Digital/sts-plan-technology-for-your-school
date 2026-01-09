import 'dotenv/config';

import importContentfulData from './helpers/import-content';

importContentfulData()
  .then(() => console.log('Import completed successfully'))
  .catch((error) => {
    console.error('Error during import:', error);
  });
