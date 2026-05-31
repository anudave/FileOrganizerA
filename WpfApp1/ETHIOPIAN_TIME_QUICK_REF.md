# ⏰ Ethiopian Time (EAT) - Quick Reference Card

## Format: HH:mm (24-hour)

### Common Times
```
00:00 = Midnight        06:00 = 6:00 AM          12:00 = Noon
01:00 = 1:00 AM         08:00 = 8:00 AM          14:00 = 2:00 PM
02:00 = 2:00 AM         09:00 = 9:00 AM          18:00 = 6:00 PM
03:00 = 3:00 AM         10:00 = 10:00 AM         20:00 = 8:00 PM
04:00 = 4:00 AM         11:00 = 11:00 AM         21:00 = 9:00 PM
05:00 = 5:00 AM         12:30 = 12:30 PM         23:59 = 11:59 PM
```

## Schedule Types

### Daily
- Runs same time every day
- Example: `08:00` runs at 8:00 AM daily

### Weekly
- Runs specific days at specific time
- Days: 0=Sun, 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri, 6=Sat
- Example: `1,3,5` = Mon, Wed, Fri at specified time

### Monthly
- Runs on specific day of month
- Example: Day 15 at `09:00` = 15th of each month at 9 AM

### Custom
- Runs every N hours
- Example: 6 = every 6 hours

## Days Reference (Weekly Only)
```
0 = Sunday       3 = Wednesday   6 = Saturday
1 = Monday       4 = Thursday
2 = Tuesday      5 = Friday
```

## Examples

### Example 1: Business Hours Daily
```
Schedule: Daily Backup
Time: 17:00 (5:00 PM)
Result: Every day at 5 PM EAT
```

### Example 2: Weekdays Morning
```
Schedule: Weekday Cleanup
Type: Weekly
Time: 08:00
Days: 1,2,3,4,5
Result: Mon-Fri at 8 AM EAT
```

### Example 3: Every 6 Hours
```
Schedule: Continuous Sync
Type: Custom
Interval: 6
Result: Every 6 hours, 24/7
```

### Example 4: Twice Daily
```
Create 2 Daily schedules:
- Morning: 06:00
- Evening: 18:00
Result: 6 AM and 6 PM every day
```

## Pro Tips
- ✅ Check "Current Time (EAT)" in status bar
- ✅ Use Run Now to test schedules
- ✅ Mark Active/Inactive via checkbox
- ✅ All times stored permanently in database
- ✅ Timezone: UTC+3 (Ethiopian Time)

## Remember
- **HH** = Hours (00-23)
- **mm** = Minutes (00-59)
- **EAT** = Ethiopian Time (UTC+3)
