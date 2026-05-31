# Ethiopian Time (EAT) - Scheduler Guide

## Overview
The File Organizer Scheduler now uses **Ethiopian Time (EAT - UTC+3)** exclusively for all scheduling operations.

---

## What is Ethiopian Time?

| Timezone | Offset | Current Offset |
|----------|--------|---|
| **Ethiopian Time (EAT)** | **UTC+3** | Current system time |
| UTC (Coordinated Universal Time) | UTC+0 | 3 hours behind EAT |
| Most of Africa | UTC+2 to UTC+4 | Varies |

---

## Time Format: HH:mm

The scheduler uses **24-hour (military) time format**.

### How to Read It:

| Time | Meaning | Example |
|------|---------|---------|
| **00:00** | Midnight (12:00 AM) | 00:15 = 12:15 AM |
| **06:00** | 6:00 AM | 06:30 = 6:30 AM |
| **12:00** | Noon (12:00 PM) | 12:45 = 12:45 PM |
| **13:00** | 1:00 PM | 14:30 = 2:30 PM |
| **18:00** | 6:00 PM | 18:00 = 6:00 PM |
| **21:00** | 9:00 PM | 21:15 = 9:15 PM |
| **23:59** | 11:59 PM | Last minute of day |

---

## Common EAT Time Examples

```
Daily Schedules:
- 06:00 = Start of business day (6:00 AM)
- 09:00 = Mid-morning (9:00 AM)
- 12:00 = Lunch time / Noon
- 17:00 = End of business day (5:00 PM)
- 21:00 = Evening (9:00 PM)
- 00:00 = Midnight (runs next day)

Weekly Schedules:
- Monday to Friday: 08:00 (8:00 AM every weekday)
- Saturday & Sunday: 10:00 (10:00 AM on weekends)
```

---

## Live Clock Display

The bottom status bar shows **Current Time (EAT)** in real-time:

```
Scheduler Status: ✓ Running (EAT) | Active Schedules: 3 | Current Time (EAT): 14:35:22
```

This displays:
- ✅ Scheduler running status
- 📊 Active schedule count
- 🕐 **Current Ethiopian Time (updates every second)**

---

## Creating Schedules

### Daily Schedule Example
- **Schedule Name:** Morning Organization
- **Type:** Daily
- **Time (HH:mm EAT):** `08:00` (8:00 AM Ethiopian Time)
- **Target Folder:** `/path/to/folder`

**Result:** Runs every day at 8:00 AM EAT

---

### Weekly Schedule Example
- **Schedule Name:** Friday Cleanup
- **Type:** Weekly
- **Time (HH:mm EAT):** `18:00` (6:00 PM Ethiopian Time)
- **Days:** `5` (Friday only)
- **Target Folder:** `/path/to/folder`

**Days Reference:**
- `0` = Sunday
- `1` = Monday
- `2` = Tuesday
- `3` = Wednesday
- `4` = Thursday
- `5` = Friday
- `6` = Saturday

**Example Values:**
- `1,3,5` = Monday, Wednesday, Friday
- `1,2,3,4,5` = Weekdays (Mon-Fri)
- `0,6` = Weekends (Sat-Sun)

---

### Custom Schedule Example
- **Schedule Name:** Every 6 Hours
- **Type:** Custom
- **Interval (Custom - hours):** `6`
- **Target Folder:** `/path/to/folder`

**Result:** Runs every 6 hours, starting from when schedule is created

---

## Important Notes

✅ **All times are in Ethiopian Time (UTC+3)**
✅ **Format is always HH:mm (24-hour)**
✅ **Current EAT time displays at bottom of scheduler**
✅ **Schedule checks run every 60 seconds**
✅ **Times are stored in database in EAT**
✅ **Run-now button executes immediately in EAT**

---

## Time Conversion Quick Reference

If you know local time and want to convert to EAT:

```
Local Time → Ethiopian Time
(assuming you're 3 hours behind EAT)

11:00 AM → 14:00 EAT
03:00 PM → 18:00 EAT
06:00 PM → 21:00 EAT
09:00 PM → 00:00 EAT (next day)
```

---

## Troubleshooting

**Q: My schedule doesn't run at the specified time**
- A: Check the "Current Time (EAT)" display to verify system time
- Ensure the schedule is marked as "Active" (checkbox in grid)
- Verify the target folder path exists

**Q: The time format is confusing**
- A: Use the examples shown in the UI: `06:00 (6AM) • 12:00 (noon) • 18:00 (6PM) • 21:00 (9PM)`
- The format helper displays at the bottom of the "Create New Schedule" section

**Q: Can I use 12-hour format (AM/PM)?**
- A: No, only 24-hour format (HH:mm) is supported
- 13:00 = 1:00 PM, 21:00 = 9:00 PM, etc.

---

## Schedule Persistence

- All schedules are stored in the database with EAT times
- Run history includes timestamp in EAT
- "Next Run" calculation uses EAT
- Last Run times display in EAT

---

## Related Features

- **FileOrganizationService:** Performs the actual file organization
- **RuleManagementService:** Manages file organization rules
- **TimeZoneService:** Handles all EAT conversions (backend)

---

For more information about creating rules or organizing files, see the main documentation.
