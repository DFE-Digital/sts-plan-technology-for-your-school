# Retrieve user data

This project reads a CSV file containing a list of DfE Sign-In user ids, calls the [Dfe Sign-In Public API](https://github.com/DFE-Digital/login.dfe.public-api?tab=readme-ov-file) for each user id, and returns a new CSV file containing user email addresses.

---

### Features

- Reads a CSV file containing a `dfeSignInRef` column
- Authenticates with the DSI Public API using JWT (HS256)
- Makes asynchronous API calls for each `dfeSignInRef`
- Extracts fields (`email`, `familyName`, `givenName`, `organisations`) from each response and iterates through `organisations` to retrieve each organisation's name and type
- Creates a timestamped `outputs-[dateTime].csv` file with the returned details appended to each row

---

### Setup

1. **Install dependencies**:

   ```bash
   npm install
   ```

2. Create a copy of the `.env.example` file, rename `.env` and populate with the client ID, API secret and endpoint. These are available from the DSI Manage console for the PTFYS service.

3. Create a `retrieve-user-data/data` folder if none exists, and within it create an `inputs.csv` file (or paste in .csv file and rename), ensuring that the `dfeSignInRef` column is populated:

   ```
   dfeSignInRef
   user-id-123
   user-id-456
   user-id-789
   ```

4. Run the script:
   ```bash
   node index.js
   ```

---

### Dependencies

- dotenv
- csv-parser
- axios
- jsonwebtoken
- json2csv

---

### Notes

- There is a `.gitignore` for this project that ignores any `.csv` files within this project to prevent accidental committing of user data.
- The JWT is valid for 5 minutes (expiresIn: '5m'), this can be adjusted if needed eg if working with large lists of IDs.
- API failures are logged and recorded in `outputs.csv` but do not stop the script.
- If no data is returned, the script will not create an outputs file.
- Secure handling and onward transfer of the `outputs.csv` file should follow DfE data protection policies, refer to the [Data Protection Hub](https://educationgovuk.sharepoint.com/sites/lvewp00158) for guidance.
