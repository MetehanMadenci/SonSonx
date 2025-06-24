const functions = require("firebase-functions");
const vision = require("@google-cloud/vision");
const cors = require("cors")({ origin: true });

const client = new vision.ImageAnnotatorClient();

exports.ocrFromImage = functions.https.onRequest((req, res) => {
  cors(req, res, async () => {
    if (req.method !== "POST") {
      return res.status(403).send("Forbidden - Only POST allowed");
    }

    const imageUrl = req.body.imageUrl;

    if (!imageUrl) {
      return res.status(400).send("Missing imageUrl");
    }

    try {
      const [result] = await client.textDetection(imageUrl);
      const detections = result.textAnnotations;
      const text = detections[0]?.description || "";
      res.status(200).json({ text });
    } catch (error) {
      console.error("OCR Error:", error);
      res.status(500).send("OCR Error: " + error.message);
    }
  });
});
