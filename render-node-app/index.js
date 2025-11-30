const express = require('express');
const app = express();
const port = process.env.PORT || 3000;

// כאן אנחנו משתמשים בפקג' שביקשו
const sdk = require('api')('@render-api/v1.0#dnrc1ulqh04b3e');

// הגדרה שתשתמש במפתח שנשמור במשתני הסביבה (לא לכתוב כאן את המפתח האמיתי!)
sdk.auth(process.env.RENDER_API_KEY);

// יצירת ה-Endpoint
app.get('/', (req, res) => {
  
  // הפקודה שמבקשת מ-Render את רשימת השירותים
  sdk.listServices({limit: '20'})
    .then(({ data }) => {
        // החזרת התשובה כ-JSON למסך
        res.json(data);
    })
    .catch(err => {
        console.error(err);
        res.status(500).send("Error fetching data");
    });
});

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});