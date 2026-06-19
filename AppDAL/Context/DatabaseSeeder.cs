using AppDAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppDAL.Context
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure database is created or migrated
            await context.Database.MigrateAsync();

            // Roles and Admin user are seeded by ApplicationSeeder in AppPL (which has BCrypt available)

            // 3. Seed 19 Pages
            var defaultPages = new List<(string Slug, string TitleEn, string TitleAr, string ContentEn, string ContentAr)>
            {
                ("home", "Home", "الرئيسية", 
                 "The Gulf & Arab General Commission for Audiovisual Media in Canada (GACAM) is a professional media organization dedicated to supporting and advancing Gulf and Arab media within the Canadian community. Through responsible media practices, professional development, and community engagement, GACAM seeks to foster integrity, transparency, and ethical communication while promoting excellence in audiovisual media.", 
                 "الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا (GACAM) هي منظمة إعلامية مهنية مكرسة لدعم وتطوير الإعلام الخليجي والعربي داخل المجتمع الكندي. من خلال الممارسات الإعلامية المسؤولة، والتطوير المهني، والمشاركة المجتمعية، تسعى الهيئة إلى تعزيز النزاهة والشفافية والتواصل الأخلاقي مع تعزيز التميز في الإعلام المرئي والمسموع."),
                
                ("about-us", "About Us", "من نحن", 
                 "The Gulf & Arab General Commission for Audiovisual Media in Canada was established to support media professionals, content creators, and organizations connected to the Gulf and Arab communities across Canada.\n\nVision:\nTo establish a professional media framework that strengthens the presence of Gulf and Arab media in Canada and promotes excellence, credibility, and responsible communication.\n\nMission:\nTo support and empower media professionals and content creators through professional development, media initiatives, and educational programs while fostering ethical and responsible media practices.\n\nCore Values:\nIntegrity, Professionalism, Independence, Accountability, Transparency, Respect for Diversity.", 
                 "تأسست الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا لدعم المهنيين وصناع المحتوى الإعلامي والمؤسسات المرتبطة بالمجتمعات الخليجية والعربية في جميع أنحاء كندا.\n\nالرؤية:\nبناء إطار إعلامي مهني يعزز حضور الإعلام الخليجي والعربي في كندا ويرسخ التميز والمصداقية والتواصل المسؤول.\n\nالرسالة:\nدعم وتمكين الإعلاميين وصناع المحتوى من خلال التطوير المهني والمبادرات الإعلامية والبرامج التعليمية مع تعزيز الممارسات الإعلامية الأخلاقية والمسؤولة.\n\nالقيم الأساسية:\nالنزاهة، المهنية، الاستقلالية، المسؤولية، الشفافية، احترام التنوع."),
                
                ("media-authority", "Media Authority", "سلطة الإعلام", 
                 "GACAM serves as a professional media platform dedicated to supporting and advancing Gulf and Arab media within the Canadian landscape.\n\nObjectives:\n- Support the development of Gulf and Arab media in Canada.\n- Promote ethical and professional media practices.\n- Empower media professionals and content creators.\n- Encourage cultural and media initiatives.\n- Foster dialogue and diversity.", 
                 "تعمل الهيئة كمنصة إعلامية مهنية مخصصة لدعم وتطوير الإعلام الخليجي والعربي في الساحة الكندية.\n\nالأهداف:\n- دعم تطوير الإعلام الخليجي والعربي في كندا.\n- تعزيز الممارسات الإعلامية الأخلاقية والمهنية.\n- تمكين المهنيين في مجال الإعلام وصناع المحتوى.\n- تشجيع المبادرات الثقافية والإعلامية.\n- تعزيز الحوار والتنوع."),
                
                ("editorial-policy", "Editorial Policy", "السياسة التحريرية", 
                 "GACAM is committed to maintaining high professional and ethical standards in all media-related activities and publications.\n\nPrinciples:\n- Accuracy and Verification\n- Independence\n- Fairness and Balance\n- Transparency and Accountability\n- Respect for Privacy\n- Rejection of Hate Speech\n- Distinction Between News and Opinion\n- Commitment to Corrections", 
                 "تلتزم الهيئة بالدفاع عن معايير مهنية وأخلاقية رفيعة في جميع الأنشطة والمنشورات الإعلامية.\n\nالمبادئ:\n- الدقة والتحقق\n- الاستقلالية\n- العدالة والتوازن\n- الشفافية والمسؤولية\n- احترام الخصوصية\n- رفض خطاب الكراهية\n- التمييز بين الخبر والرأي\n- الالتزام بالتصحيح"),
                
                ("complaints-policy", "Complaints Policy", "سياسة الشكاوى", 
                 "GACAM welcomes feedback and complaints concerning its content, services, or professional conduct.\nComplaints may be submitted through the official contact channels provided on the website.\nDepending on the circumstances, the Commission may:\n- Provide clarification.\n- Correct or update content.\n- Publish a correction when appropriate.\n- Decline the complaint with an explanation.", 
                 "ترحب الهيئة بالآراء والملاحظات والشكاوى المتعلقة بمحتواها أو خدماتها أو سلوكها المهني.\nيمكن تقديم الشكاوى عبر قنوات الاتصال الرسمية المتاحة على الموقع الإلكتروني.\nوبناءً على الظروف، يجوز للهيئة:\n- تقديم التوضيحات اللازمة.\n- تصحيح المحتوى أو تحديثه.\n- نشر تصحيح عندما يكون ذلك مناسباً.\n- رفض الشكوى مع تقديم التفسير المناسب."),
                
                ("corrections-policy", "Corrections Policy", "سياسة التصحيح", 
                 "GACAM is committed to accuracy, transparency, and accountability.\nWhen errors are identified, reasonable efforts are made to correct or clarify the information in a timely manner.\nCorrections may include:\n- Updates.\n- Clarifications.\n- Editorial notes.\n- Published corrections.", 
                 "تلتزم الهيئة بالدقة والشفافية والمسؤولية.\nوعند تحديد أي أخطاء، تبذل الهيئة جهوداً معقولة لتصحيح المعلومات وتوضيحها في الوقت المناسب.\nقد تشمل التصحيحات:\n- التحديثات.\n- التوضيحات.\n- الملاحظات التحريرية.\n- ونشر التصحيحات."),
                
                ("code-of-ethics", "Code of Ethics", "ميثاق الشرف الإعلامي", 
                 "The Commission is guided by ethical principles that promote integrity, professionalism, and responsible communication.\nKey Principles:\n- Integrity\n- Accuracy\n- Independence\n- Transparency\n- Respect for Privacy\n- Respect for Diversity\n- Professional Responsibility", 
                 "تسترشد الهيئة بمبادئ أخلاقية تعزز النزاهة والمهنية والتواصل المسؤول.\nالمبادئ الأساسية:\n- النزاهة\n- الدقة\n- الاستقلالية\n- الشفافية\n- احترام الخصوصية\n- احترام التنوع\n- المسؤولية المهنية"),
                
                ("services", "Services", "الخدمات", 
                 "GACAM provides a range of professional programs and services intended to support media professionals and content creators.\nServices Include:\n- Media Accreditation\n- Media Cards\n- Training and Professional Development\n- Professional Certificates\n- Media Consultations\n- Conferences and Forums\n- Cultural and Media Initiatives\n- News and Press Releases", 
                 "تقدم الهيئة مجموعة من البرامج والخدمات المهنية التي تهدف إلى دعم الإعلاميين وصناع المحتوى.\nتشمل الخدمات:\n- الاعتماد الإعلامي\n- البطاقات الإعلامية\n- التدريب والتطوير المهني\n- الشهادات المهنية\n- الاستشارات الإعلامية\n- المؤتمرات والمنتديات\n- المبادرات الثقافية والإعلامية\n- الأخبار والبيانات الصحفية"),
                
                ("media-accreditation", "Media Accreditation", "الاعتماد الإعلامي", 
                 "GACAM provides professional media accreditation programs intended to support media professionals and content creators.\nSubmission of an application does not guarantee approval.\nPayment of fees does not constitute automatic acceptance.\nMedia accreditation issued by the Commission is intended to identify professional affiliation with GACAM and does not constitute a government-issued license or regulatory authorization.", 
                 "تقدم الهيئة برامج اعتماد إعلامي مهنية لدعم الإعلاميين وصناع المحتوى.\nتقديم الطلب لا يضمن الموافقة عليه تلقائياً.\nدفع الرسوم لا يعني القبول التلقائي.\nيهدف الاعتماد الإعلامي الصادر عن الهيئة إلى تحديد الانتساب المهني للهيئة ولا يمثل ترخيصاً حكومياً أو تفويضاً تنظيمياً."),
                
                ("media-id-verification", "Media ID Verification", "التحقق من الهوية الإعلامية", 
                 "Media cards issued by GACAM may be verified electronically through:\n- Card Number.\n- QR Code.\n\nCard Status:\n- Active\n- Expired\n- Suspended\n- Revoked", 
                 "يمكن التحقق من البطاقات الإعلامية الصادرة عن الهيئة إلكترونياً عبر:\n- رقم البطاقة.\n- رمز الاستجابة السريعة (QR Code).\n\nحالة البطاقة تشمل:\n- نشطة\n- منتهية الصلاحية\n- معلقة\n- ملغاة"),
                
                ("training-programs", "Training Programs", "البرامج التدريبية", 
                 "GACAM provides educational and professional development programs designed to enhance skills, knowledge, and responsible media practices.\nIncludes:\n- Course Registration.\n- Training Programs.\n- Electronic Certificates.\n- Printed Certificates.\n- Certificate Verification.\n- QR Verification.", 
                 "توفر الهيئة برامج تعليمية وتطوير مهني تهدف لتعزيز المهارات والمعرفة والممارسات الإعلامية المسؤولة.\nتشمل:\n- التسجيل في الدورات.\n- البرامج التدريبية.\n- الشهادات الإلكترونية.\n- الشهادات المطبوعة.\n- التحقق من الشهادات.\n- التحقق عبر رمز الاستجابة السريعة."),
                
                ("volunteers", "Volunteers", "المتطوعون", 
                 "GACAM welcomes individuals who wish to contribute to media, educational, cultural, and community initiatives through volunteer participation.\nVolunteer Areas:\n- Media and Journalism\n- Photography and Production\n- Public Relations\n- Event Management\n- Translation and Editing\n- Design and Creative Services\n- Digital Media\n- Training Programs\n- Administrative Support", 
                 "ترحب الهيئة بالأفراد الراغبين في المساهمة بالمبادرات الإعلامية والتعليمية والثقافية والمجتمعية من خلال المشاركة التطوعية.\nمجالات التطوع:\n- الإعلام والصحافة\n- التصوير والإنتاج\n- العلاقات العامة\n- إدارة الفعاليات\n- الترجمة والتحرير\n- التصميم والخدمات الإبداعية\n- الإعلام الرقمي\n- برامج التدريب\n- الدعم الإداري"),
                
                ("partners", "Partners", "الشركاء", 
                 "GACAM values collaboration and recognizes the importance of building meaningful relationships with organizations and institutions that share similar goals and values.\nPartnership Categories:\n- Strategic Partners\n- Supporting Organizations\n- Community Partners\n- Media Partners\n- Educational and Cultural Institutions", 
                 "تثمن الهيئة التعاون وتدرك أهمية بناء علاقات هادفة مع المنظمات والمؤسسات التي تشاركها الأهداف والقيم ذاتها.\nفئات الشراكة:\n- الشركاء الاستراتيجيون\n- المؤسسات الداعمة\n- الشركاء المجتمعيون\n- الشركاء الإعلاميون\n- المؤسسات التعليمية والثقافية"),
                
                ("news-press-releases", "News & Press Releases", "الأخبار والبيانات الصحفية", 
                 "News and press releases form an important part of GACAM’s commitment to transparency and public engagement.\nSections may include:\n- News\n- Press Releases\n- Announcements\n- Statements\n- Events & Forums\n- Initiatives", 
                 "تشكل الأخبار والبيانات الصحفية جزءاً مهماً من التزام الهيئة بالشفافية والتفاعل العام.\nقد تشمل الأقسام:\n- الأخبار\n- البيانات الصحفية\n- الإعلانات\n- التصريحات\n- الأحداث والمنتديات\n- المبادرات"),
                
                ("leadership-board-of-directors", "Leadership & Board of Directors", "القيادة ومجلس الإدارة", 
                 "GACAM is committed to sound governance, transparency, and institutional accountability.\nThe Board of Directors and Executive Leadership provide strategic oversight and guidance in support of the Commission’s mission and objectives.", 
                 "تلتزم الهيئة بالحوكمة السليمة والشفافية والمسؤولية المؤسسية. يقدم مجلس الإدارة والقيادة التنفيذية الإشراف والتوجيه الاستراتيجي دعماً لرسالة الهيئة وأهدافها."),
                
                ("faq", "FAQ", "الأسئلة الشائعة", 
                 "Frequently Asked Questions.\nThis section provides answers regarding:\n- Media Accreditation\n- Media Cards\n- Training Programs\n- Certificates\n- Volunteer Opportunities\n- Policies and Procedures\n- Contact Information", 
                 "الأسئلة الشائعة.\nيقدم هذا القسم إجابات تتعلق بـ:\n- الاعتماد الإعلامي\n- البطاقات الإعلامية\n- البرامج التدريبية\n- الشهادات\n- الفرص التطوعية\n- السياسات والإجراءات\n- معلومات الاتصال"),
                
                ("contact-us", "Contact Us", "اتصل بنا", 
                 "Contact Us.\nOfficial Email:\nInfo@gacam.media\n\nContact Form:\n- Full Name\n- Email Address\n- Subject\n- Message", 
                 "اتصل بنا.\nالبريد الإلكتروني الرسمي:\nInfo@gacam.media\n\nنموذج الاتصال:\n- الاسم الكامل\n- البريد الإلكتروني\n- الموضوع\n- الرسالة"),
                
                ("terms-of-use", "Terms of Use", "شروط الاستخدام", 
                 "By accessing this website, users agree to comply with the applicable Terms of Use.\nThe website and its services are intended for lawful and professional purposes.\nAll materials are protected by intellectual property rights.\nGACAM reserves the right to modify these Terms of Use at any time.", 
                 "شروط الاستخدام.\nمن خلال الدخول إلى هذا الموقع، يوافق المستخدمون على الالتزام بشروط الاستخدام المعمول بها.\nالموقع وخدماته مخصصان لأغراض مشروعة ومهنية.\nجميع المواد محمية بحقوق الملكية الفكرية.\nتحتفظ الهيئة بالحق في تعديل شروط الاستخدام هذه في أي وقت."),
                
                ("privacy-policy", "Privacy Policy", "سياسة الخصوصية", 
                 "GACAM respects the privacy of users and is committed to protecting personal information collected through its website.\nPersonal information is used solely for administrative, operational, and communication purposes.\nGACAM does not sell personal information and does not disclose it to third parties except where required by law or with the consent of the individual concerned.", 
                 "سياسة الخصوصية.\nتحترم الهيئة خصوصية المستخدمين وتلتزم بحماية المعلومات الشخصية التي يتم جمعها من خلال موقعها الإلكتروني.\nتُستخدم المعلومات الشخصية فقط لأغراض إدارية وتشغيلية وتواصلية.\nلا تقوم الهيئة ببيع المعلومات الشخصية ولا تفصح عنها لأطراف ثالثة إلا بموجب القانون أو بموافقة الشخص المعني.")
            };

            foreach (var pageInfo in defaultPages)
            {
                var existingPage = await context.Pages.FirstOrDefaultAsync(p => p.Slug == pageInfo.Slug);
                if (existingPage != null)
                {
                    existingPage.TitleEn = pageInfo.TitleEn;
                    existingPage.TitleAr = pageInfo.TitleAr;
                    existingPage.ContentEn = pageInfo.ContentEn;
                    existingPage.ContentAr = pageInfo.ContentAr;
                    existingPage.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    await context.Pages.AddAsync(new Page
                    {
                        Slug = pageInfo.Slug,
                        TitleEn = pageInfo.TitleEn,
                        TitleAr = pageInfo.TitleAr,
                        ContentEn = pageInfo.ContentEn,
                        ContentAr = pageInfo.ContentAr,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // 5. Seed Accreditation Categories
            await SeedAccreditationCategoriesAsync(context);

            // Service Fees
            if (!await context.ServiceFees.AnyAsync())
            {
                await context.ServiceFees.AddRangeAsync(
                    new ServiceFee
                    {
                        OrderType = OrderType.CertificatePrint,
                        UnitPrice = 100,
                        ShippingFee = 25,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ServiceFee
                    {
                        OrderType = OrderType.AccreditationCardPrint,
                        UnitPrice = 150,
                        ShippingFee = 30,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );

                await context.SaveChangesAsync();
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedAccreditationCategoriesAsync(AppDbContext context)
        {
            var categories = new List<(string NameEn, string NameAr)>
            {
                ("Press", "صحافة"),
                ("Media", "إعلام"),
                ("Staff", "فريق العمل"),
                ("Organizer", "منظم"),
                ("Speaker", "متحدث"),
                ("Guest", "ضيف"),
                ("VIP", "شخصية مهمة"),
                ("Trainee", "متدرب"),
                ("Volunteer", "متطوع"),
                ("Board Member", "عضو مجلس إدارة"),
                ("Executive", "تنفيذي"),
                ("Honorary", "عضو شرفي"),
                ("Partner", "شريك")
            };

            var seedDate = DateTime.UtcNow;
            for (int i = 0; i < categories.Count; i++)
            {
                var (nameEn, nameAr) = categories[i];
                var displayOrder = i + 1;

                var existing = await context.AccreditationCategories
                    .FirstOrDefaultAsync(c => c.DisplayOrder == displayOrder);

                if (existing != null)
                {
                    existing.NameEn = nameEn;
                    existing.NameAr = nameAr;
                    existing.IsActive = true;
                }
                else
                {
                    await context.AccreditationCategories.AddAsync(new AccreditationCategory
                    {
                        NameEn = nameEn,
                        NameAr = nameAr,
                        IsActive = true,
                        DisplayOrder = displayOrder,
                        CreatedAt = seedDate
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
