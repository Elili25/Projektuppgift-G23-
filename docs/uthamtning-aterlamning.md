# Uthämtning och återlämning utanför bokningsfönstret

Vad som händer när en bil hämtas ut eller lämnas tillbaka utanför den bokade tiden (`StartDate`–`EndDate`).
Reglerna ligger i `RentalReturnService` (`RegisterPickup`, `RegisterReturn`) och gäller alla anrop. Frontenden visar dem men avgör ingenting själv.

## Beslut

| Situation | Beslut |
|---|---|
| Uthämtning tidigare än 1 h före `StartDate` | **Blockeras** |
| Återlämning före `EndDate` (tidig) | **Tillåts**, personalen får en varning i frontend |
| Återlämning efter `EndDate` (sen) | **Tillåts och flaggas** med förseningsavgift |

## Uthämtning

- `RegisterPickup` kastar `InvalidOperationException` om uthämtningen sker tidigare än `EarliestPickupBeforeStart` (1 h) före `StartDate`.
- Gränsen räknas ut på ett ställe: `RentalReturnService.EarliestPickupTime(bookingStart)`.
- Frontend (`Views/BookingManagement/Pickup.cshtml`) får tidpunkten från servern, visar när uthämtning öppnar och inaktiverar knappen innan dess. JS blockerar också submit före tidpunkten.
- Mellan öppningstid och `StartDate` visas en `confirm()`.

## Återlämning

`RegisterReturn` avvisar:

- lägre mätarställning än vid uthämtning
- returtid mer än 1 minut i framtiden
- returtid före den faktiska uthämtningen

Sen återlämning flaggas genom att förseningsavgiften sparas på bokningen (`booking.LateFee`, beräknad av `PricingServices.CalculateLateFee`) och att händelsen `LateReturn` skickas.

### Beslut: tidig återlämning tillåts

Tidig återlämning blockeras inte, av dessa skäl:

1. **Den är redan betald.** Kunden har betalat för hela bokningsperioden. Att lämna tillbaka tidigare skadar inte hyresgivaren.
2. **Bilen blir ledig.** `RegisterReturn` sätter `vehicle.IsAvailable = true`. Att tvinga kvar bilen till `EndDate` skulle låsa en bil som står oanvänd.
3. **Det är en verklig situation.** Personal ska kunna registrera att bilen står på plats, annars finns risk att bilen står kvar som uthyrd i systemet.
4. **Det skiljer sig från tidig uthämtning.** Tidig uthämtning kan krocka med föregående bokning av samma bil. Tidig återlämning kan inte det.

Eftersom det är ovanligt får personalen en varning (`Views/Returns/Register.cshtml`): om returtiden är före bokad slutpunkt (`data-planned-end`) visas en `confirm()` innan formuläret skickas. Ingen avgift tas ut och ingen återbetalning görs automatiskt.

### Konsekvenser

- Ingen avgift och ingen återbetalning för outnyttjad tid. Det skulle kräva ett nytt beslut.
- Varningen finns bara i frontend. Servern avvisar inte tidig återlämning, vilket är avsikten.
