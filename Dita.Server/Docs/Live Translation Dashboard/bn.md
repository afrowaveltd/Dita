# লাইভ অনুবাদ ড্যাশবোর্ড

লাইভ অনুবাদ বোর্ড একটি প্রশাসক পাতা যা স্বয়ংক্রিয় অনুবাদের পাইপলাইনে বাস্তব সময় প্রদর্শন করে থাকে। এটা সিগন্যালআর হাবের সাথে সংযোগ করে আর সব পাইপলাইন ইভেন্ট দেখায়।.

## ইউ- আর- এল

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## IMAP-র বৈশিষ্ট্য

### বাস্তব সময়-অঞ্চল:

সব সংকেত এই অনুবাদ পাইপ-এর অনুষ্ঠান সরাসরি চালু করা টেবিলে প্রদর্শিত হয়েছে:

- **Sequence number** — Monotonic counter within each pipeline run
- **Tetpt** — অনুষ্ঠান গ্রহণ করার সময় স্থানীয় সময়
- **Run ID** — Shortened GUID for correlation
- **Stut** - পাইপলাইন ব্যাজ (Comples, অনুবাদ, ইত্যাদি)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- ****s-কিছু বোধগম্য নয়
- **Con** - ইভেন্টের সম্পূর্ণ মূল্য

### রঙের এনকোডিং

রঙ
|-------|---------|
নীল
সবুজ
লাল
সাদা (ডিফল্ট)

### সংযোগের অবস্থা

উপরের শো-এ একটি স্ট্যাটাস ব্যানার:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

সংযোগগুলি SalutLOPL-র সাথে স্বয়ংক্রিয় পুনরায় সংযোগ করে: 0, 2, 5, 10, 30.

### নিয়ন্ত্রণ

- **Clear Feed** — Removes all displayed messages and resets the counter
- ** JSON** - সকল বার্তা বিশ্লেষণের জন্য JSON ফাইল হিসাবে গ্রহণ করা হবে
- **Message counter** — Shows total number of events received in this session

## ক্লাস রবিউস

ড্যাশবোর্ড- এর সাথে সংযোগ স্থাপন করেছে:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### বার্তা তালিকা

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

### ইভেন্টের ধরন

ড্যাশবোর্ড দ্বারা সব মান চিহ্নিত করা হয়:

ধরন
|------|---------|
নীল ব্যাজ
সবুজ ব্যাজ
রেড ব্যাজ
সবুজ ব্যাজ
রেড ব্যাজ
ইনফো ব্যাজ
সতর্কবার্তা ব্যাজ

## প্রযুক্তিগত বাস্তবায়ন

### ব্যাক-এন্ড

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### সম্মুখপ্রান্ত

- বুটস্ট্র্যাপ ৫- এর মাধ্যমে বিশুদ্ধ HTML/jS টুল
- মাইক্রোসফট সিগন্যাল আউটপুট জাভাস্ক্রীপ্ট ক্লায়েন্ট (ডিএন.এন থেকে লোড করা হয়)
- ইভেন্টের ফিডের জন্য কোনো সার্ভার উপস্থিত নেই

### পৃষ্ঠার গঠন

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## উন্নয়নের সময় ব্যবহার

1. ডিটা চালু করো। অ্যাপ্লিকেশন সার্ভার
2. পরিবর্তীত হয়েছে
3. অনুবাদ চালান (অথবা সিডিউলার অথবা API কল করা জন্য অপেক্ষা করুন)
4. সময় নিরীক্ষণ করুন
5. ডিবাগ করার জন্য এক্সপোর্ট বাটন ব্যবহার করুন

## ভবিষ্যৎকে উন্নত করে

ড্যাশবোর্ডের জন্য পরিকল্পনা:

- **Authentication** — Restrict access to users with the `Admin` role
- **Fing** — মঞ্চ, ধরন, সঞ্চালন অথবা পরিচালনা করুন
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **-ম্যান দ্বারা স্বয়ং ধার্য বিশেষ পাইপলাইন আরম্ভ করার উদ্দেশ্যে **
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## সমস্যার সমাধান

### ড্যাশবোর্ড প্রদর্শন

1. সার্ভার চলছে কি না তা পরীক্ষা করুন
2. বিপরীত শব্দ অথবা নেটওয়ার্ক ত্রুটির জন্য ব্রাউজার কনসোল পরীক্ষা করুন
3. নিশ্চিত করা হয়েছে
4. ফায়ারওয়াল দ্বারা কোনো ফায়ারওয়াল বন্ধ করা হয়নি

### ঘটনা পাওয়া যাচ্ছে না

1. যে সিগনাল- টি সার্ভার - এ বিদ্যমান ইউ. আর. এল ( সার্ভার ও ক্লায়েন্টের সাথে মিল রয়েছে)
2. সময় নির্ধারণের ব্যবস্থা সক্রিয় রয়েছে কি না
3. অনুবাদ পাইপ ত্রুটির জন্য সার্ভার লগ ইন করুন
4. ব্রাউজার পরীক্ষা করুন ওয়েব ব্যবহারকারীদের জন্য নেটওয়ার্ক ট্যাব

### বার্তা তালিকা

মাঠের মধ্যে একটা এককের আদেশ আছে। পরিচিত ব্যক্তি থেকে বার্তা প্রাপ্ত হয়েছে:
- একাধিক পাইপলাইন চালিত করে (রিম্যাপহোরে লক করার সময় এটি হওয়া উচিত নয়)
- ব্রাউজারের মূল সমস্যা (ছবি সতেজ করার উদ্দেশ্যে)
