# लाइव अनुवाद डैशबोर्ड

लाइव अनुवाद डैशबोर्ड एक व्यवस्थापक पृष्ठ है जो स्वचालित अनुवाद पाइपलाइन में वास्तविक समय दृश्यता प्रदान करता है। यह सिग्नलआर हब से जोड़ता है और वे होने के रूप में सभी पाइपलाइन घटनाओं को प्रदर्शित करता है।.

## यूआरएल

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## सुविधाएँ

### रियल टाइम इवेंट स्ट्रीम

अनुवाद पाइपलाइन से सभी सिग्नलआर कार्यक्रम लाइव-अपडेटिंग टेबल में प्रदर्शित होते हैं:

- **Sequence number** — Monotonic counter within each pipeline run
- ** टाइमस्टैम्प ** - जब घटना प्राप्त हुई थी तब स्थानीय समय
- **Run ID** - संक्षिप्त GUID सहसंबंध के लिए
- ** स्टेज ** - पाइपलाइन स्टेज बैज (चेकसर्वर्स, ट्रांसलेटकॉंटरी आदि)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### रंग कोडिंग

रंग
|-------|---------|
नीला
हरा ()
लाल
सफेद (डिफ़ॉल्ट)

### कनेक्शन की स्थिति

शीर्ष शो में एक स्थिति बैनर:
- **Connecting** — Establishing SignalR connection
- ** कनेक्ट ** - सामान्य रूप से घटनाओं को प्राप्त करना
- ** कनेक्ट करना ** - कनेक्शन खो गया, फिर से कनेक्ट करने का प्रयास
- ** डिस्कनेक्ट ** - कनेक्शन बंद

कनेक्शन एक्सोनेंशियल बैकऑफ के साथ स्वचालित रीकनेक्ट का उपयोग करता है: 0s, 2s, 5s, 10s, 30s।.

### नियंत्रण

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** - सभी को विश्लेषण के लिए JSON फ़ाइल के रूप में संदेश प्राप्त हुआ
- **Message counter** — Shows total number of events received in this session

## सिग्नलआर हब

डैशबोर्ड से जुड़ जाता है:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### संदेश अनुबंध

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### घटना प्रकार

डैशबोर्ड सभी मूल्यों को संभालता है:

प्रकार
|------|---------|
ब्लू बिल्ला
ग्रीन बिल्ला
लाल बिल्ला
ग्रीन बिल्ला
लाल बिल्ला
जानकारी बैज
चेतावनी बैज

## तकनीकी कार्यान्वयन

### बैकएंड

- **LocalizationHub ** () - सिग्नलआर हब जो सभी जुड़े ग्राहकों को संदेश प्रसारित करता है
- ** ISignalRPublisher ** - अनुवाद सेवाओं में उपयोग के लिए हब पर प्रतिबंध
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### फ्रंटेंड

- बूटस्ट्रैप 5 स्टाइल के साथ शुद्ध एचटीएमएल / जेएस
- Microsoft SignalR जावास्क्रिप्ट क्लाइंट लाइब्रेरी (CDN से लोड) का उपयोग करता है
- घटना फ़ीड के लिए कोई सर्वर-साइड रेंडरिंग आवश्यक नहीं है

### पृष्ठ संरचना

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## विकास के दौरान प्रयोग

1. दीटा शुरू करें। सर्वर अनुप्रयोग
2. नेविगेट करने के लिए
3. एक अनुवाद रन ट्रिगर करें (या तो शेड्यूलर की प्रतीक्षा करें या एपीआई को कॉल करें)
4. वास्तविक समय में घटनाओं को देखें
5. डीबगिंग के लिए एक पूर्ण निशान पर कब्जा करने के लिए निर्यात बटन का उपयोग करें

## भविष्य में वृद्धि

डैशबोर्ड के लिए योजनाबद्ध सुधार:

- **Authentication** — Restrict access to users with the `Admin` role
- **फ़िल्टर ** - चरण, प्रकार, या चलाने वाली ID द्वारा फ़िल्टर इवेंट
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- ** मैनुअल ट्रिगर ** - बटन मैन्युअल रूप से विशिष्ट पाइपलाइन चरणों को शुरू करने के लिए
- ** कॉन्फ़िगरेशन ** - डैशबोर्ड से सीधे संपादित करें
- ** भाषा प्रबंधन ** - समर्थित भाषाओं को देखें और संपादित करें
- **Dictionary preview** - ब्राउज़ करें और स्थानीयकरण शब्दकोशों की खोज

## समस्या निवारण

### डैशबोर्ड "कनेक्ट करने में विफल" दिखाता है

1. सत्यापित करें कि सर्वर चल रहा है और सुलभ है
2. CORS या नेटवर्क त्रुटियों के लिए ब्राउज़र कंसोल की जाँच करें
3. पुष्टि में मौजूद है
4. सुनिश्चित करें कि कोई फायरवॉल WebSocket कनेक्शन को अवरुद्ध नहीं कर रहा है

### घटनाओं दिखाई नहीं दे रहे हैं

1. जाँच करें कि सिग्नलआर हब यूआरएल सर्वर () और क्लाइंट () के बीच मेल खाता है
2. शेड्यूलर को सत्यापित करने में सक्षम है
3. अनुवाद पाइपलाइन त्रुटियों के लिए सर्वर लॉग को देखें
4. WebSocket संदेशों के लिए ब्राउज़र नेटवर्क टैब की जाँच करें

### संदेश आदेश से बाहर हैं

फील्ड एक ही रन के भीतर ऑर्डर करने की गारंटी देता है। यदि संदेश क्रम से बाहर दिखाई देते हैं, तो यह संकेत दे सकता है:
- एकाधिक पाइपलाइन ओवरलैपिंग चलाता है ( semaphore लॉक के कारण नहीं होना चाहिए)
- ब्राउज़र रेंडरिंग मुद्दे (पृष्ठ ताज़ा करना)
