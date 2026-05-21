# Sitemap - CarRent.Web

| URL | Controller | Action | View |
| --- | --- | --- | --- |
| `/` | Home | Index | Views/Home/Index.cshtml |
| `/pocetna` | Home | Index | Views/Home/Index.cshtml |
| `/Home/Index` | Home | Index | Views/Home/Index.cshtml |
| `/vozni-park` | Fleet | Index | Views/Fleet/Index.cshtml |
| `/Fleet/Index` | Fleet | Index | Views/Fleet/Index.cshtml |
| `/dnevni-plan` | DailyPlan | Index | Views/DailyPlan/Index.cshtml |
| `/operativa/dnevni-plan` | DailyPlan | Index | Views/DailyPlan/Index.cshtml |
| `/raspored` | Timeline | Index | Views/Timeline/Index.cshtml |
| `/raspored/mjesecni` | Timeline | Index | Views/Timeline/Index.cshtml |
| `/partneri` | Partners | Index | Views/Partners/Index.cshtml |
| `/partneri/novi` | Partners | Create | Views/Partners/Create.cshtml |
| `/partneri/uredi/{id}` | Partners | Edit | Views/Partners/Edit.cshtml |
| `/BranchOffice/Index` | BranchOffice | Index | Views/BranchOffice/Index.cshtml |
| `/BranchOffice/Details/{id}` | BranchOffice | Details | Views/BranchOffice/Details.cshtml |
| `/Vehicle/Index` | Vehicle | Index | Views/Vehicle/Index.cshtml |
| `/Vehicle/Details/{id}` | Vehicle | Details | Views/Vehicle/Details.cshtml |
| `/vozila/reg/{registration}` | Vehicle | ByRegistration | redirect na Details |
| `/Customer/Index` | Customer | Index | Views/Customer/Index.cshtml |
| `/Customer/Details/{id}` | Customer | Details | Views/Customer/Details.cshtml |
| `/Reservation/Index` | Reservation | Index | Views/Reservation/Index.cshtml |
| `/Reservation/Details/{id}` | Reservation | Details | Views/Reservation/Details.cshtml |
| `/rezervacije/pregled/{id}` | Reservation | Details | Views/Reservation/Details.cshtml |
| `/Addon/Index` | Addon | Index | Views/Addon/Index.cshtml |
| `/Addon/Details/{id}` | Addon | Details | Views/Addon/Details.cshtml |
| `/ServiceRecord/Index` | ServiceRecord | Index | Views/ServiceRecord/Index.cshtml |
| `/ServiceRecord/Details/{id}` | ServiceRecord | Details | Views/ServiceRecord/Details.cshtml |
| `/Employee/Index` | Employee | Index | Views/Employee/Index.cshtml |
| `/Employee/Details/{id}` | Employee | Details | Views/Employee/Details.cshtml |

## Partial viewovi

- `Views/Shared/_Layout.cshtml` - layout
- `Views/Shared/_PageHeaderPartial.cshtml` - naslov sekcije
- `Views/Shared/_PartnerFormPartial.cshtml` - create/edit forma partnera
