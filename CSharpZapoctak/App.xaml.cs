using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using CSharpZapoctak.ViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpZapoctak
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //create databases if they do not exist
            CreateSportsManagerDatabase();

            //create folders for images, if they do not exist
            if (!Directory.Exists(SportsData.AppDataPath))
            {
                Directory.CreateDirectory(SportsData.AppDataPath);
            }
            if (!Directory.Exists(SportsData.ImagesPath))
            {
                Directory.CreateDirectory(SportsData.ImagesPath);
            }
            if (!Directory.Exists(SportsData.CompetitionLogosPath))
            {
                Directory.CreateDirectory(SportsData.CompetitionLogosPath);
            }
            if (!Directory.Exists(SportsData.SeasonLogosPath))
            {
                Directory.CreateDirectory(SportsData.SeasonLogosPath);
            }
            if (!Directory.Exists(SportsData.TeamLogosPath))
            {
                Directory.CreateDirectory(SportsData.TeamLogosPath);
            }
            if (!Directory.Exists(SportsData.PlayerPhotosPath))
            {
                Directory.CreateDirectory(SportsData.PlayerPhotosPath);
            }

            //automatically starts mysql
            if (Process.GetProcessesByName("mysqld").Length == 0)
            {
                //Process.Start("C:/xampp/mysql/bin/mysqld.exe");
            }

            Task.Run(() => LoadCountries());

            NavigationStore navigationStore = new NavigationStore();

            //starts with SportsSelectionView
            navigationStore.CurrentViewModel = new SportsSelectionViewModel(navigationStore);

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationStore)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //Process.GetProcessesByName("mysqld")[0].Kill();

            base.OnExit(e);
        }

        private void LoadCountries()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=sports_manager;UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT code_two , name , code_three FROM country", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow cntry in dataTable.Rows)
                {
                    Country c = new Country
                    {
                        Name = cntry["name"].ToString(),
                        CodeTwo = cntry["code_two"].ToString(),
                        CodeThree = cntry["code_three"].ToString()
                    };
                    SportsData.countries.Add(c);
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CreateSportsManagerDatabase()
        {
            string query = "CREATE DATABASE IF NOT EXISTS `sports_manager`;"; string a =
"            CREATE TABLE `country` (                                                               " +
"  `code_three` char(3) NOT NULL,                                                                   " +
"  `name` varchar(200) NOT NULL,                                                                    " +
"  `code_two` char(2) NOT NULL                                                                      " +
") ENGINE = InnoDB DEFAULT CHARSET = utf8;                                                          " +
"                                                                                                   " +
"            INSERT INTO `country` (`code_three`, `name`, `code_two`) VALUES                        " +
"            ('AND', 'Andorra', 'AD'),                                                              " +
"('ARE', 'United Arab Emirates', 'AE'),                                                             " +
"('AFG', 'Afghanistan', 'AF'),                                                                      " +
"('ATG', 'Antigua and Barbuda', 'AG'),                                                              " +
"('AIA', 'Anguilla', 'AI'),                                                                         " +
"('ALB', 'Albania', 'AL'),                                                                          " +
"('ARM', 'Armenia', 'AM'),                                                                          " +
"('AGO', 'Angola', 'AO'),                                                                           " +
"('ATA', 'Antarctica', 'AQ'),                                                                       " +
"('ARG', 'Argentina', 'AR'),                                                                        " +
"('ASM', 'American Samoa', 'AS'),                                                                   " +
"('AUT', 'Austria', 'AT'),                                                                          " +
"('AUS', 'Australia', 'AU'),                                                                        " +
"('ABW', 'Aruba', 'AW'),                                                                            " +
"('ALA', 'Åland', 'AX'),                                                                            " +
"('AZE', 'Azerbaijan', 'AZ'),                                                                       " +
"('BIH', 'Bosnia and Herzegovina', 'BA'),                                                           " +
"('BRB', 'Barbados', 'BB'),                                                                         " +
"('BGD', 'Bangladesh', 'BD'),                                                                       " +
"('BEL', 'Belgium', 'BE'),                                                                          " +
"('BFA', 'Burkina Faso', 'BF'),                                                                     " +
"('BGR', 'Bulgaria', 'BG'),                                                                         " +
"('BHR', 'Bahrain', 'BH'),                                                                          " +
"('BDI', 'Burundi', 'BI'),                                                                          " +
"('BEN', 'Benin', 'BJ'),                                                                            " +
"('BLM', 'Saint Barthélemy', 'BL'),                                                                 " +
"('BMU', 'Bermuda', 'BM'),                                                                          " +
"('BRN', 'Brunei', 'BN'),                                                                           " +
"('BOL', 'Bolivia', 'BO'),                                                                          " +
"('BES', 'Bonaire', 'BQ'),                                                                          " +
"('BRA', 'Brazil', 'BR'),                                                                           " +
"('BHS', 'Bahamas', 'BS'),                                                                          " +
"('BTN', 'Bhutan', 'BT'),                                                                           " +
"('BVT', 'Bouvet Island', 'BV'),                                                                    " +
"('BWA', 'Botswana', 'BW'),                                                                         " +
"('BLR', 'Belarus', 'BY'),                                                                          " +
"('BLZ', 'Belize', 'BZ'),                                                                           " +
"('CAN', 'Canada', 'CA'),                                                                           " +
"('CCK', 'Cocos [Keeling] Islands', 'CC'),                                                          " +
"('COD', 'Democratic Republic of the Congo', 'CD'),                                                 " +
"('CAF', 'Central African Republic', 'CF'),                                                         " +
"('COG', 'Republic of the Congo', 'CG'),                                                            " +
"('CHE', 'Switzerland', 'CH'),                                                                      " +
"('CIV', 'Ivory Coast', 'CI'),                                                                      " +
"('COK', 'Cook Islands', 'CK'),                                                                     " +
"('CHL', 'Chile', 'CL'),                                                                            " +
"('CMR', 'Cameroon', 'CM'),                                                                         " +
"('CHN', 'China', 'CN'),                                                                            " +
"('COL', 'Colombia', 'CO'),                                                                         " +
"('CRI', 'Costa Rica', 'CR'),                                                                       " +
"('CUB', 'Cuba', 'CU'),                                                                             " +
"('CPV', 'Cape Verde', 'CV'),                                                                       " +
"('CUW', 'Curacao', 'CW'),                                                                          " +
"('CXR', 'Christmas Island', 'CX'),                                                                 " +
"('CYP', 'Cyprus', 'CY'),                                                                           " +
"('CZE', 'Czech Republic', 'CZ'),                                                                   " +
"('DEU', 'Germany', 'DE'),                                                                          " +
"('DJI', 'Djibouti', 'DJ'),                                                                         " +
"('DNK', 'Denmark', 'DK'),                                                                          " +
"('DMA', 'Dominica', 'DM'),                                                                         " +
"('DOM', 'Dominican Republic', 'DO'),                                                               " +
"('DZA', 'Algeria', 'DZ'),                                                                          " +
"('ECU', 'Ecuador', 'EC'),                                                                          " +
"('EST', 'Estonia', 'EE'),                                                                          " +
"('EGY', 'Egypt', 'EG'),                                                                            " +
"('ESH', 'Western Sahara', 'EH'),                                                                   " +
"('ERI', 'Eritrea', 'ER'),                                                                          " +
"('ESP', 'Spain', 'ES'),                                                                            " +
"('ETH', 'Ethiopia', 'ET'),                                                                         " +
"('FIN', 'Finland', 'FI'),                                                                          " +
"('FJI', 'Fiji', 'FJ'),                                                                             " +
"('FLK', 'Falkland Islands', 'FK'),                                                                 " +
"('FSM', 'Micronesia', 'FM'),                                                                       " +
"('FRO', 'Faroe Islands', 'FO'),                                                                    " +
"('FRA', 'France', 'FR'),                                                                           " +
"('GAB', 'Gabon', 'GA'),                                                                            " +
"('GBR', 'United Kingdom', 'GB'),                                                                   " +
"('GRD', 'Grenada', 'GD'),                                                                          " +
"('GEO', 'Georgia', 'GE'),                                                                          " +
"('GUF', 'French Guiana', 'GF'),                                                                    " +
"('GGY', 'Guernsey', 'GG'),                                                                         " +
"('GHA', 'Ghana', 'GH'),                                                                            " +
"('GIB', 'Gibraltar', 'GI'),                                                                        " +
"('GRL', 'Greenland', 'GL'),                                                                        " +
"('GMB', 'Gambia', 'GM'),                                                                           " +
"('GIN', 'Guinea', 'GN'),                                                                           " +
"('GLP', 'Guadeloupe', 'GP'),                                                                       " +
"('GNQ', 'Equatorial Guinea', 'GQ'),                                                                " +
"('GRC', 'Greece', 'GR'),                                                                           " +
"('SGS', 'South Georgia and the South Sandwich Islands', 'GS'),                                     " +
"('GTM', 'Guatemala', 'GT'),                                                                        " +
"('GUM', 'Guam', 'GU'),                                                                             " +
"('GNB', 'Guinea-Bissau', 'GW'),                                                                    " +
"('GUY', 'Guyana', 'GY'),                                                                           " +
"('HKG', 'Hong Kong', 'HK'),                                                                        " +
"('HMD', 'Heard Island and McDonald Islands', 'HM'),                                                " +
"('HND', 'Honduras', 'HN'),                                                                         " +
"('HRV', 'Croatia', 'HR'),                                                                          " +
"('HTI', 'Haiti', 'HT'),                                                                            " +
"('HUN', 'Hungary', 'HU'),                                                                          " +
"('IDN', 'Indonesia', 'ID'),                                                                        " +
"('IRL', 'Ireland', 'IE'),                                                                          " +
"('ISR', 'Israel', 'IL'),                                                                           " +
"('IMN', 'Isle of Man', 'IM'),                                                                      " +
"('IND', 'India', 'IN'),                                                                            " +
"('IOT', 'British Indian Ocean Territory', 'IO'),                                                   " +
"('IRQ', 'Iraq', 'IQ'),                                                                             " +
"('IRN', 'Iran', 'IR'),                                                                             " +
"('ISL', 'Iceland', 'IS'),                                                                          " +
"('ITA', 'Italy', 'IT'),                                                                            " +
"('JEY', 'Jersey', 'JE'),                                                                           " +
"('JAM', 'Jamaica', 'JM'),                                                                          " +
"('JOR', 'Jordan', 'JO'),                                                                           " +
"('JPN', 'Japan', 'JP'),                                                                            " +
"('KEN', 'Kenya', 'KE'),                                                                            " +
"('KGZ', 'Kyrgyzstan', 'KG'),                                                                       " +
"('KHM', 'Cambodia', 'KH'),                                                                         " +
"('KIR', 'Kiribati', 'KI'),                                                                         " +
"('COM', 'Comoros', 'KM'),                                                                          " +
"('KNA', 'Saint Kitts and Nevis', 'KN'),                                                            " +
"('PRK', 'North Korea', 'KP'),                                                                      " +
"('KOR', 'South Korea', 'KR'),                                                                      " +
"('KWT', 'Kuwait', 'KW'),                                                                           " +
"('CYM', 'Cayman Islands', 'KY'),                                                                   " +
"('KAZ', 'Kazakhstan', 'KZ'),                                                                       " +
"('LAO', 'Laos', 'LA'),                                                                             " +
"('LBN', 'Lebanon', 'LB'),                                                                          " +
"('LCA', 'Saint Lucia', 'LC'),                                                                      " +
"('LIE', 'Liechtenstein', 'LI'),                                                                    " +
"('LKA', 'Sri Lanka', 'LK'),                                                                        " +
"('LBR', 'Liberia', 'LR'),                                                                          " +
"('LSO', 'Lesotho', 'LS'),                                                                          " +
"('LTU', 'Lithuania', 'LT'),                                                                        " +
"('LUX', 'Luxembourg', 'LU'),                                                                       " +
"('LVA', 'Latvia', 'LV'),                                                                           " +
"('LBY', 'Libya', 'LY'),                                                                            " +
"('MAR', 'Morocco', 'MA'),                                                                          " +
"('MCO', 'Monaco', 'MC'),                                                                           " +
"('MDA', 'Moldova', 'MD'),                                                                          " +
"('MNE', 'Montenegro', 'ME'),                                                                       " +
"('MAF', 'Saint Martin', 'MF'),                                                                     " +
"('MDG', 'Madagascar', 'MG'),                                                                       " +
"('MHL', 'Marshall Islands', 'MH'),                                                                 " +
"('MKD', 'Macedonia', 'MK'),                                                                        " +
"('MLI', 'Mali', 'ML'),                                                                             " +
"('MMR', 'Myanmar [Burma]', 'MM'),                                                                  " +
"('MNG', 'Mongolia', 'MN'),                                                                         " +
"('MAC', 'Macao', 'MO'),                                                                             " +
"('MNP', 'Northern Mariana Islands', 'MP'),                                                          " +
"('MTQ', 'Martinique', 'MQ'),                                                                        " +
"('MRT', 'Mauritania', 'MR'),                                                                        " +
"('MSR', 'Montserrat', 'MS'),                                                                        " +
"('MLT', 'Malta', 'MT'),                                                                             " +
"('MUS', 'Mauritius', 'MU'),                                                                         " +
"('MDV', 'Maldives', 'MV'),                                                                          " +
"('MWI', 'Malawi', 'MW'),                                                                            " +
"('MEX', 'Mexico', 'MX'),                                                                            " +
"('MYS', 'Malaysia', 'MY'),                                                                          " +
"('MOZ', 'Mozambique', 'MZ'),                                                                        " +
"('NAM', 'Namibia', 'NA'),                                                                           " +
"('NCL', 'New Caledonia', 'NC'),                                                                     " +
"('NER', 'Niger', 'NE'),                                                                             " +
"('NFK', 'Norfolk Island', 'NF'),                                                                    " +
"('NGA', 'Nigeria', 'NG'),                                                                           " +
"('NIC', 'Nicaragua', 'NI'),                                                                         " +
"('NLD', 'Netherlands', 'NL'),                                                                       " +
"('NOR', 'Norway', 'NO'),                                                                            " +
"('NPL', 'Nepal', 'NP'),                                                                             " +
"('NRU', 'Nauru', 'NR'),                                                                             " +
"('NIU', 'Niue', 'NU'),                                                                              " +
"('NZL', 'New Zealand', 'NZ'),                                                                       " +
"('OMN', 'Oman', 'OM'),                                                                              " +
"('PAN', 'Panama', 'PA'),                                                                            " +
"('PER', 'Peru', 'PE'),                                                                              " +
"('PYF', 'French Polynesia', 'PF'),                                                                  " +
"('PNG', 'Papua New Guinea', 'PG'),                                                                  " +
"('PHL', 'Philippines', 'PH'),                                                                       " +
"('PAK', 'Pakistan', 'PK'),                                                                          " +
"('POL', 'Poland', 'PL'),                                                                            " +
"('SPM', 'Saint Pierre and Miquelon', 'PM'),                                                         " +
"('PCN', 'Pitcairn Islands', 'PN'),                                                                  " +
"('PRI', 'Puerto Rico', 'PR'),                                                                       " +
"('PSE', 'Palestine', 'PS'),                                                                         " +
"('PRT', 'Portugal', 'PT'),                                                                          " +
"('PLW', 'Palau', 'PW'),                                                                             " +
"('PRY', 'Paraguay', 'PY'),                                                                          " +
"('QAT', 'Qatar', 'QA'),                                                                             " +
"('REU', 'Réunion', 'RE'),                                                                           " +
"('ROU', 'Romania', 'RO'),                                                                           " +
"('SRB', 'Serbia', 'RS'),                                                                            " +
"('RUS', 'Russia', 'RU'),                                                                            " +
"('RWA', 'Rwanda', 'RW'),                                                                            " +
"('SAU', 'Saudi Arabia', 'SA'),                                                                      " +
"('SLB', 'Solomon Islands', 'SB'),                                                                   " +
"('SYC', 'Seychelles', 'SC'),                                                                        " +
"('SDN', 'Sudan', 'SD'),                                                                             " +
"('SWE', 'Sweden', 'SE'),                                                                            " +
"('SGP', 'Singapore', 'SG'),                                                                         " +
"('SHN', 'Saint Helena', 'SH'),                                                                      " +
"('SVN', 'Slovenia', 'SI'),                                                                          " +
"('SJM', 'Svalbard and Jan Mayen', 'SJ'),                                                            " +
"('SVK', 'Slovakia', 'SK'),                                                                          " +
"('SLE', 'Sierra Leone', 'SL'),                                                                      " +
"('SMR', 'San Marino', 'SM'),                                                                        " +
"('SEN', 'Senegal', 'SN'),                                                                           " +
"('SOM', 'Somalia', 'SO'),                                                                           " +
"('SUR', 'Suriname', 'SR'),                                                                          " +
"('SSD', 'South Sudan', 'SS'),                                                                       " +
"('STP', 'São Tomé and Príncipe', 'ST'),                                                             " +
"('SLV', 'El Salvador', 'SV'),                                                                       " +
"('SXM', 'Sint Maarten', 'SX'),                                                                      " +
"('SYR', 'Syria', 'SY'),                                                                             " +
"('SWZ', 'Swaziland', 'SZ'),                                                                         " +
"('TCA', 'Turks and Caicos Islands', 'TC'),                                                          " +
"('TCD', 'Chad', 'TD'),                                                                              " +
"('ATF', 'French Southern Territories', 'TF'),                                                       " +
"('TGO', 'Togo', 'TG'),                                                                              " +
"('THA', 'Thailand', 'TH'),                                                                          " +
"('TJK', 'Tajikistan', 'TJ'),                                                                        " +
"('TKL', 'Tokelau', 'TK'),                                                                           " +
"('TLS', 'East Timor', 'TL'),                                                                        " +
"('TKM', 'Turkmenistan', 'TM'),                                                                      " +
"('TUN', 'Tunisia', 'TN'),                                                                           " +
"('TON', 'Tonga', 'TO'),                                                                             " +
"('TUR', 'Turkey', 'TR'),                                                                            " +
"('TTO', 'Trinidad and Tobago', 'TT'),                                                               " +
"('TUV', 'Tuvalu', 'TV'),                                                                            " +
"('TWN', 'Taiwan', 'TW'),                                                                            " +
"('TZA', 'Tanzania', 'TZ'),                                                                          " +
"('UKR', 'Ukraine', 'UA'),                                                                           " +
"('UGA', 'Uganda', 'UG'),                                                                            " +
"('UMI', 'U.S. Minor Outlying Islands', 'UM'),                                                       " +
"('USA', 'United States', 'US'),                                                                     " +
"('URY', 'Uruguay', 'UY'),                                                                           " +
"('UZB', 'Uzbekistan', 'UZ'),                                                                        " +
"('VAT', 'Vatican City', 'VA'),                                                                      " +
"('VCT', 'Saint Vincent and the Grenadines', 'VC'),                                                  " +
"('VEN', 'Venezuela', 'VE'),                                                                         " +
"('VGB', 'British Virgin Islands', 'VG'),                                                            " +
"('VIR', 'U.S. Virgin Islands', 'VI'),                                                               " +
"('VNM', 'Vietnam', 'VN'),                                                                           " +
"('VUT', 'Vanuatu', 'VU'),                                                                           " +
"('WLF', 'Wallis and Futuna', 'WF'),                                                                 " +
"('WSM', 'Samoa', 'WS'),                                                                             " +
"('XKX', 'Kosovo', 'XK'),                                                                            " +
"('YEM', 'Yemen', 'YE'),                                                                             " +
"('MYT', 'Mayotte', 'YT'),                                                                           " +
"('ZAF', 'South Africa', 'ZA'),                                                                      " +
"('ZMB', 'Zambia', 'ZM'),                                                                            " +
"('ZWE', 'Zimbabwe', 'ZW');                                                                          " +
"                                                                                                    " +
"            ALTER TABLE `country`                                                                   " +
"  ADD PRIMARY KEY(`code_two`) USING BTREE;                                                          " +
"            COMMIT;                                                                                 ";
            string connectionString = "SERVER=" + SportsData.server + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (System.Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}