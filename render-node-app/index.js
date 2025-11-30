const express = require('express');
const app = express();
const port = process.env.PORT || 3000;

app.get('/', async (req, res) => {
  const apiKey = process.env.RENDER_API_KEY;

  if (!apiKey) {
    console.error("Error: RENDER_API_KEY is missing!");
    return res.status(500).send("Server Error: Missing API Key");
  }

  try {
    // פנייה ישירה ל-API של Render
    const response = await fetch('https://api.render.com/v1/services?limit=20', {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
        'Authorization': `Bearer ${apiKey}`
      }
    });

    if (!response.ok) {
       throw new Error(`Render API responded with status: ${response.status}`);
    }

    const data = await response.json();
    res.json(data);

  } catch (err) {
    console.error("Error details:", err);
    res.status(500).send("Error fetching data from Render");
  }
});

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});