using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Xml.Linq;
using programSaptamanal;
using System.Data.Entity.Validation;

namespace programSaptamanal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            importFRHEntities db = new importFRHEntities(); //baza de date


            // Șterge datele vechi din tabel
            var existingRecords = db.importFrhs.ToList();
            if (existingRecords.Any())
            {
                db.importFrhs.RemoveRange(existingRecords);
                db.SaveChanges();
            }


            IWebDriver driver = new ChromeDriver();

            // gasirea linkului cu programul saptamanal
            driver.Navigate().GoToUrl(@"https://frh.ro");
            Thread.Sleep(5000);
            IWebElement programLink = driver.FindElement(By.XPath("//a[contains(text(), 'Program saptamanal')]"));
            string programUrl = programLink.GetAttribute("href");
            //

            //inlocuiesc @"link" cu programUrl
            driver.Navigate().GoToUrl(programUrl); //site 
            Thread.Sleep(5000);

            // Clasa tabel
            string tabelaClasa = "table program-table";

            // echipe bune
            List<string> echipePH = new List<string> { "CSM Ploiesti", "CSO Tricolorul Breaza", "ACS Academia Junior Ploiesti", "CS Brazi", "CS Judetean Prahova", "CSO Mizil", "CS Bucov", "CSS Municipiul Ploiesti", "CS Campina", "HC Activ CSO Plopeni", "ACS Gloria 2024 Baicoi" };

            // Găsim toate elementele <tr> din tabel
            IList<IWebElement> toateRandurile = driver.FindElements(By.XPath("//table[contains(@class, '" + tabelaClasa + "')]/tbody/tr"));

            Console.WriteLine("Numărul de rânduri găsite: " + toateRandurile.Count);

            // Parcurgem fiecare rând pentru a căuta textul în primul <td>
            for (int i = 0; i < toateRandurile.Count; i++)
            {
                // Găsim primul celulă (td) din rândul curent
                IWebElement a3aCelula = toateRandurile[i].FindElement(By.XPath("./td[3]"));

                // Verificăm dacă textul din prima celulă conține oricare din elementele din lista saliPH
                foreach (string echipaP in echipePH)
                {
                    if (a3aCelula.Text.ToLower().Contains(echipaP.ToLower()) && !a3aCelula.Text.ToLower().Contains("sta"))
                    {
                        importFrh ifrh = new importFrh(); //ancora la tabel

                        Console.WriteLine("------");

                        //calea catre tbody ul celulei
                        IWebElement tbodyRelevant = a3aCelula.FindElement(By.XPath("ancestor::tbody"));

                        //calea catre theadul acestui tbody - categorie competitie
                        IWebElement theadAsociat = tbodyRelevant.FindElement(By.XPath("preceding::thead[1]"));


                        // Verificăm dacă thead-ul a fost găsit
                        if (theadAsociat != null)
                        {
                            // Găsim al doilea tr din thead-ul asociat
                            IWebElement alDoileaTrThead = theadAsociat.FindElement(By.XPath(".//tr[2]")); // Indexul 2 pentru al doilea tr

                            // Accesăm al doilea td din al doilea tr și afișăm textul său
                            IList<IWebElement> toateCeluleleThead = alDoileaTrThead.FindElements(By.XPath("./td"));
                            if (toateCeluleleThead.Count > 1)
                            {
                                IWebElement alDoileaTDCelulaThead = toateCeluleleThead[1]; // Indexul 1 pentru al doilea td
                                Console.WriteLine(alDoileaTDCelulaThead.Text);

                                try
                                {
                                    ifrh.categorieMeci = alDoileaTDCelulaThead.Text; //import in tabel
                                }
                                catch (Exception)
                                {
                                    ifrh.categorieMeci = "Nu sunt disponibile informatii suplimentare";
                                }
                            }
                            else //in cazul in care sunt doua etape in eceeasi categorie, a doua etapa nu are specificata categoria in thead si apelam thead ul anterior
                            {
                                theadAsociat= tbodyRelevant.FindElement(By.XPath("preceding::thead[2]"));
                                alDoileaTrThead = theadAsociat.FindElement(By.XPath(".//tr[2]"));
                                toateCeluleleThead = alDoileaTrThead.FindElements(By.XPath("./td"));
                                IWebElement alDoileaTDCelulaThead = toateCeluleleThead[1]; // Indexul 1 pentru al doilea td
                                Console.WriteLine(alDoileaTDCelulaThead.Text);

                                try
                                {
                                    ifrh.categorieMeci = alDoileaTDCelulaThead.Text; //import in tabel
                                }
                                catch (Exception)
                                {
                                    ifrh.categorieMeci = "Nu sunt disponibile informatii suplimentare";
                                }
                            }

                        }


                        //randul cu meciul 
                        if (i > 0)
                        {
                            Console.WriteLine(toateRandurile[i].Text);

                            //variabile cu fiecare celula din rand
                            IWebElement dataM = toateRandurile[i].FindElement(By.XPath("./td[1]"));
                            IWebElement oraM = toateRandurile[i].FindElement(By.XPath("./td[2]"));
                            IWebElement echipa1M = toateRandurile[i].FindElement(By.XPath("./td[3]"));
                            IWebElement echipa2M = toateRandurile[i].FindElement(By.XPath("./td[5]")); // skip 4 pt "vs"

                            //import variabile la tabel
                            ifrh.dataMeci = dataM.Text;
                            ifrh.oraMeci = oraM.Text;
                            ifrh.echipa1 = echipa1M.Text;
                            ifrh.echipa2 = echipa2M.Text;
                        }

                        // randul cu sala i + 1
                        Console.WriteLine(toateRandurile[i + 1].Text);

                        //variabila pentru sala si import la tabel
                        IWebElement salaM = toateRandurile[i + 1].FindElement(By.XPath("./td[1]"));
                        ifrh.salaMeci = salaM.Text.Substring(6); ;

                        db.importFrhs.Add(ifrh);

                        //aici schimb
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var validationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                                }
                            }
                            throw; // Re-aruncă excepția pentru a vedea orice eroare
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("O eroare a apărut: " + ex.Message);
                            throw; // Re-aruncă excepția pentru a vedea ce se întâmplă
                        }

                        //

                        break;
                    }
                }
            }

            //Console.ReadLine();

            driver.Quit();
            ExportPDF();
            Console.ReadLine();
        }

        static void ExportPDF()
        {
            importFRHEntities db = new importFRHEntities();
            string caleFisier = "D:\\pdfTester\\program_s";
            List<importFrh> lista = db.importFrhs.ToList();

            using (var fileStream = new FileStream(caleFisier, FileMode.Create))
            {
                using (var writer = new PdfWriter(fileStream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        foreach (importFrh ifrh in lista)
                        {
                            document.Add(new Paragraph(ifrh.categorieMeci + "\n" + ifrh.dataMeci + " " + ifrh.oraMeci + " " + ifrh.salaMeci + " - " + ifrh.echipa1 + " vs " + ifrh.echipa2 + "\n\n"));
                        }
                    }
                }
            }

            Console.WriteLine("\nFisierul PDF a fost generat cu succes la adresa: " + caleFisier);
        }
    }
}
