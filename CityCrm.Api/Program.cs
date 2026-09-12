using CityCrm.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<CityCrm.Api.Services.OsmService>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    context.Database.Migrate();
    
    if (!context.Users.Any())
    {
        var defUser = builder.Configuration["DefaultAdmin:Username"];
        var defPass = builder.Configuration["DefaultAdmin:Password"];

        if (defUser == "Set_In_User_Secrets" || string.IsNullOrEmpty(defUser)) defUser = "admin_fallback";
        if (defPass == "Set_In_User_Secrets" || string.IsNullOrEmpty(defPass)) defPass = "FallbackPass123!";

        context.Users.Add(new CityCrm.Api.Entities.User 
        { 
            Username = defUser, 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(defPass),
            Role = "GrandAdmin" 
        });
        context.SaveChanges();
    }
    
    if (!context.Streets.Any())
    {
        var seedFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "streets.json");
        if (File.Exists(seedFilePath))
        {
            var jsonData = File.ReadAllText(seedFilePath);
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var streets = System.Text.Json.JsonSerializer.Deserialize<List<CityCrm.Api.Entities.Street>>(jsonData, options);

            if (streets != null && streets.Any())
            {
                context.Streets.AddRange(streets);
                context.SaveChanges();
            }
        }
    }
    
    if (!context.Buildings.Any() && context.Streets.Any())
    {
        var firstStreet = context.Streets.First(s => s.Name == "Миру");
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        var testBuilding = new CityCrm.Api.Entities.Building
        {
            StreetId = firstStreet.Id,
            BuildingNumber = 15,
            BuildingType = "Багатоповерхівка",
            Condition = "В експлуатації", 
            Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(31.2953, 51.4938)),
            
            Premises = new List<CityCrm.Api.Entities.Premise>
            {
                new CityCrm.Api.Entities.Premise 
                { 
                    PremiseNumber = "Офіс 1", Area = 120.5, Type = "Комерційна", 
                    Status = "Вільне", Ownership = "Комунальна" 
                }
            }
        };
        
        context.Buildings.Add(testBuilding);
        context.SaveChanges();
    }

    if (!context.Murals.Any())
    {
        context.Murals.AddRange(
            new CityCrm.Api.Entities.Mural { 
                Title = "Льодовиковий період", 
                Address = "просп. Левка Лук'яненка, 14 (на стіні при заїзді у двір)", 
                Lat = 51.510706, Lng = 31.328251, 
                Artist = "Віталій Гідеван (@vitaliy_gide1)",
                Description = "Вуличний арт.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Ice+Age" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Сови", 
                Address = "м-н Стара Подусівка, вул. Харківська, 8", 
                Lat = 51.493760, Lng = 31.248396, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "На малюнку зображена мати-сова зі своїми совенятками, яких вона обіймає крилом, і одночасно показує навколишній світ.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Owls" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Котики", 
                Address = "м-н Ремзавод, просп. Миру, 198 (кав’ярня Coffee Boss)", 
                Lat = 51.523422, Lng = 31.272127, 
                Artist = "Андрій Козир",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Cats" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Маленький Хуліган", 
                Address = "м-н Дитинець, вул. Воздвиженська, 5 (нанесено на щитовій)", 
                Lat = 51.492080, Lng = 31.304975, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2016 р. Автобіографічний образ маленького хулігана, який покинув свої іграшки і зрозумів, чим займатиметься далі.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Hooligan" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Кит", 
                Address = "м-н Дитинець, просп. Миру, 13", 
                Lat = 51.488520, Lng = 31.303514, 
                Artist = "Magic Art Group (Луцьк)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Whale" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Єнот", 
                Address = "просп. Миру, 51 (Стіна кінотеатру «Дружба»)", 
                Lat = 51.499364, Lng = 31.287895, 
                Artist = "Віталій Гідеван (@vitaliy_gide1)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Raccoon" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Ластівки", 
                Address = "просп. Миру, 211 (стіна онкодиспансеру)", 
                Lat = 51.527723, Lng = 31.275729, 
                Artist = "Сергій Тонканов (@serhiytonkanov)",
                Description = "На стіні намальовані ластівки на фоні блакитного неба. Ластівка символізує надію. Коли літає високо – до доброї погоди і до одужання. Це символічно, тому що мурал розташований на стіні Чернігівського обласного онкодиспансеру.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Swallows" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Леонід Каденюк", 
                Address = "м-н Ялівщина, вул. Льотна, 4", 
                Lat = 51.516126, Lng = 31.277242, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2017 р. Зображений Леонід Каденюк – перший космонавт незалежної України. Робота створена в рамках проекту «До тебе моє місто промовляє».",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Kadenyuk" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Лисиця", 
                Address = "м-н ЦУМ, просп. Миру, 51 (Стіна кінотеатру «Дружба»)", 
                Lat = 51.499530, Lng = 31.288511, 
                Artist = "Віталій Гідеван (@vitaliy_gide1)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Fox" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Тигрята", 
                Address = "м-н Градецький, просп. Миру, 61", 
                Lat = 51.501957, Lng = 31.284239, 
                Artist = "Роман Синенко, Олексій Бичек",
                Description = "Натюрморт з апельсинами та тиграми. Мурал виконаний в техніці плоскої графіки.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Tigers" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Дівчина", 
                Address = "м-н Масани, вул. Незалежності, 56", 
                Lat = 51.522432, Lng = 31.230771, 
                Artist = "Роман Синенко, Олексій Бичек",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Girl" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Княжна Чорна", 
                Address = "м-н Масани, вул. Петлюри, 23а", 
                Lat = 51.516112, Lng = 31.245671, 
                Artist = "Роман Синенко, Олексій Бичек",
                Description = "Зображена дочка легендарного чернігівського князя Чорного. Княжна Чорна була горда і прекрасна дівчина-амазонка з трагічною долею.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Princess" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Портрет Козака", 
                Address = "вул. Козацька, 13а", 
                Lat = 51.512033, Lng = 31.269902, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2017 р. Мурал у світло-коричневих тонах зображає мужнього козака із сергою у вусі на фоні поля битви. Намальовано на честь нової назви вулиці (колишня 50 років ВЛКСМ).",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Cossack" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Їжачок", 
                Address = "м-н Голлівуд, просп. Михайла Грушевського, 157", 
                Lat = 51.515224, Lng = 31.310560, 
                Artist = "Віталій Гідеван (@vitaliy_gide1)",
                Description = "Барвистий уквітчаний їжачок. Художник створив його на честь народження своєї доньки у Чернігові. «Їжачок несе букет квітів, а в ньому курча» - пояснив автор.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Hedgehog" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Вікна", 
                Address = "перехрестя просп. Перемоги та вул. Княжа", 
                Lat = 51.492506, Lng = 31.293002, 
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Windows" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Місто", 
                Address = "вул. Коцюбинського, 8а (фасад школи №20)", 
                Lat = 51.488434, Lng = 31.286808, 
                Artist = "Popay (Франція)",
                Description = "Створено у 2017 р. в межах проекту Mural Social Club. Поєднання урбаністичних пейзажів Парижа і Чернігова: багатоповерхівки, магістралі, вітражі у вікнах.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=City" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Янголятко з котиком", 
                Address = "парк Б. Хмельницького, вул. Гетьмана Полуботка, 16/6", 
                Lat = 51.493631, Lng = 31.301799, 
                Artist = "Євгенія Гапчинська, Віталій Гідеван",
                Description = "Створено у 2017 р. Героями муралу стали допитлива дівчинка та її котик, що виконані в характерному стилі Гапчинської.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Angel" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "As Flowers", 
                Address = "парк Б. Хмельницького, вул. Гетьмана Полуботка, 12", 
                Lat = 51.492838, Lng = 31.300600, 
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=As+Flowers" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Вишиванка", 
                Address = "м-н Єлецька гірка, вул. Реміснича (стіна с/м Союз)", 
                Lat = 51.490828, Lng = 31.291053, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2017 р.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Vyshyvanka" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "1-ша танкова бригада", 
                Address = "просп. Перемоги", 
                Lat = 51.495417, Lng = 31.296967, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2022 р. Присвячений захисникам міста.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=1st+Tank+Brigade" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Міські легенди", 
                Address = "м-н Рокосовського, просп. Левка Лук'яненка 37", 
                Lat = 51.512155, Lng = 31.325035, 
                Artist = "Дмитро Шляпов",
                Description = "Створено у 2014 р. Зображені фрагменти з легенд: привид Мотрі Кочубей, стародавні дуби, ченець з Антонієвих печер, сотник Дунін-Борковський.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Legends" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = " Мати з дитиною", 
                Address = "вул. 1-го Травня, 1 (Пологовий будинок)", 
                Lat = 51.517453, Lng = 31.310603, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Maternity" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Зелений Коник", 
                Address = "м-н Епіцентр, вул. 1-ї танкової бригади, 8", 
                Lat = 51.520442, Lng = 31.317443, 
                Artist = "Олексій Бичек, Роман Синенко",
                Description = "Створено у 2016 р. Яскравий кінь в етнічному стилі, виконаний у приємних кольорах, які піднімають настрій.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Green+Horse" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Козак Мамай", 
                Address = "м-н Епіцентр, вул. 1-ї танкової бригади, 8", 
                Lat = 51.519068, Lng = 31.317178, 
                Artist = "Олексій Бичек, Роман Синенко",
                Description = "Створено у 2016 р. Козак із бандурою в етнічному стилі. «Він є власником Зеленого коня, який намальований поруч, та пливе до нього».",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Cossack+Mamay" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Бесіда", 
                Address = "м-н Нива, вул. Всіхсвятська, 14 (стіна школи №30)", 
                Lat = 51.513792, Lng = 31.332259, 
                Artist = "Mono Gonzales (@monogonsalezchile)",
                Description = "Абстрактна картина у різнобарвних кольорах, яка символізує культурний обмін між людьми. Автор - піонер муралізму в Латинській Америці.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Conversation" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Осінь – новий старт", 
                Address = "паркан біля Чернігівської політехніки, вул. Шевченка, 95", 
                Lat = 51.505291, Lng = 31.335428, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Autumn" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Квітучі руки", 
                Address = "м-н Єлецька Гірка, вул. Княжа, 16", 
                Lat = 51.489110, Lng = 31.296912, 
                Artist = "Олександр Бритцев (@britcev_oleksander)",
                Description = "Створено в межах загальноукраїнського проекту. «Людські руки тягнуться одна до одної, щоб об'єднатися. Квіти яблуні – це надія на відродження життя».",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Hands" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Перше тепло", 
                Address = "м-н Єськова, Стрілецька набережна, 88", 
                Lat = 51.526468, Lng = 31.291322, 
                Artist = "Сергій Тонканов",
                Description = "Створено у 2021 р.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Warmth" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Етнопара", 
                Address = "вул. Мазепи, 37", 
                Lat = 51.488855, Lng = 31.275781, 
                Artist = "Олексій Бичек, Роман Синенко",
                Description = "Створено у 2016 р. Закохані українець та українка в національному одязі. Виконано в техніці плоскої графіки.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Ethno+Couple" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Портрет", 
                Address = "вул. Коцюбинського, 54", 
                Lat = 51.492386, Lng = 31.295771, 
                Artist = "Родріго Бранко",
                Description = "Створено у 2016 р. Портрет хлопця з синім волоссям. «Свій мурал я присвятив енергетиці українців», – прокоментував автор з Бразилії.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Portrait" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Андрій Ярмоленко", 
                Address = "м-н 5 кутів, просп. Перемоги, 110 (ДЮСШ «Юність»)", 
                Lat = 51.499307, Lng = 31.303672, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2020 р. Присвячений вихованцю цієї школи, відомому українському футболісту. Ескіз погоджений особисто з Андрієм Ярмоленком.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Yarmolenko" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "АЗОВці", 
                Address = "вул. Тероборони, 15 (колишній магазин «Квартал»)", 
                Lat = 51.498800, Lng = 31.253855, 
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Azov" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Акваріум", 
                Address = "біля Річпорту", 
                Lat = 51.484838, Lng = 31.309226, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Aquarium" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Місто мрій", 
                Address = "м-н 5 кутів, просп. Перемоги, 108", 
                Lat = 51.498203, Lng = 31.301406, 
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Dream+City" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Наталі", 
                Address = "Колона пішохідного мосту", 
                Lat = 51.481912, Lng = 31.311579, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2018 р.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Natali" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Купальський сюжет Юлія", 
                Address = "м-н Річпорт, опори пішохідного мосту через Десну", 
                Lat = 51.481995, Lng = 31.311238, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2017 р.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Kupala" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "LOVE VANDALISM", 
                Address = "Автомобільний міст", 
                Lat = 51.453794, Lng = 31.304856, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2016 р. «Що для когось вандалізм, а для когось кохання усього життя».",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Vandalism" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Арт-укриття", 
                Address = "пляж Золотий Берег", 
                Lat = 51.489065, Lng = 31.319450, 
                Artist = "Віталій Вороний",
                Description = "Роботи митця-рятувальника, який самостійно навчився працювати в стилі графіті й аерографії.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Shelter" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Мурал пам’яті", 
                Address = "м-н Нива, просп. Левка Лук'яненка 22а (обласна бібліотека)", 
                Lat = 51.513974, Lng = 31.321619, 
                Artist = "Віталій Вороний",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Memory" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Херсон - це Україна", 
                Address = "біля ТЦ ЦУМ", 
                Lat = 51.499106, Lng = 31.291636, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2022 р. Серія патріотичних графіті. Меседж – підтримка мешканців окупованих рашистами міст.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Kherson" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Євген Коновалець", 
                Address = "біля ТЦ ЦУМ, на стіні кінотеатру «Дружба»", 
                Lat = 51.499276, Lng = 31.288193, 
                Description = "Створено у 2026 р. студентами кафедри архітектури та дизайну Чернігівської політехніки за ініціативи ГО «Ветеранський корпус».",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Konovalets" 
            },
            new CityCrm.Api.Entities.Mural { 
                Title = "Біцуха", 
                Address = "вул. Козацька 4б (школа №15)", 
                Lat = 51.511500, Lng = 31.275000, 
                Artist = "Дмитро Адеріхо (@Ader_One)",
                Description = "Створено у 2022 р. Мурал-пам'ять захиснику України Віталію Трухану на псевдо «Біцуха». Зроблено, щоб діти бачили, що тут вчився Герой, а не просто чорну меморіальну дошку.",
                PhotoUrl = "https://placehold.co/600x400/6f42c1/FFFFFF?text=Bitsukha" 
            }
        );
        context.SaveChanges();
    
    }
}

app.Run();