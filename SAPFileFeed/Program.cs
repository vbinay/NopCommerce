using FlatFile.FixedLength.Implementation;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tamir.SharpSsh.jsch;

using Tamir.SharpSsh;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Tamir.SharpSsh.Wrappers;

namespace SAPFileFeed
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static SapFileFeed dbEntity = new SapFileFeed();

        public static void Main(string[] args)
        {
           
            SAPFileFeed.RunSAPFeed();
        }

        public static void readXML(string xml)
        {

            string result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header></soapenv:Header><soapenv:Body><VertexEnvelope xmlns=\"urn:vertexinc:o-series:tps:8:0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Login><UserName>testuser02</UserName> <Password>aTA#Xu3k</Password> </Login> <DistributeTaxResponse documentNumber=\"31906\" documentDate=\"2018-10-09\" transactionType=\"SALE\"><Seller><Company>1000</Company> <Division>U101</Division> <PhysicalOrigin taxAreaId=\"140318411\"><StreetAddress1>525 South Racine</StreetAddress1> <City>Chicago</City> <MainDivision>IL</MainDivision> <PostalCode>60607</PostalCode> <Country>US</Country> </PhysicalOrigin> </Seller> <Customer><CustomerCode>1</CustomerCode> <Destination taxAreaId=\"330612010\"><StreetAddress1>21 West 52nd Street</StreetAddress1> <City>New York</City> <MainDivision>NY</MainDivision> <PostalCode>10021</PostalCode> <Country>US</Country> </Destination> </Customer> <SubTotal>61.99</SubTotal> <Total>64.21</Total> <TotalTax>2.22</TotalTax> <LineItem lineItemId=\"17564\" lineItemNumber=\"1\"><Product productClass=\"1000\">Birthday Cake 2</Product> <Quantity>1.0</Quantity> <ExtendedPrice>32.0</ExtendedPrice> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" filingCategoryCode=\"6\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"STATE\" jurisdictionId=\"24354\">NEW YORK</Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>32.0</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\">Sales and Compensating Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>79460</TaxRuleId> <InclusionRuleId>1293080</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"CITY\" jurisdictionId=\"25353\">NEW YORK</Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>32.0</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\">Local Sales and Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>259039</TaxRuleId> <InclusionRuleId>1293087</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"DISTRICT\" jurisdictionId=\"79774\"><![CDATA[METROPOLITAN COMMUTER TRANSPORTATION DISTRICT]]></Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>32.0</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\"><![CDATA[Metropolitan Commuter Transportation District]]></Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>75714</TaxRuleId> <InclusionRuleId>1293725</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <TotalTax>0.0</TotalTax> </LineItem> <LineItem lineItemId=\"17608\" lineItemNumber=\"2\"><Product productClass=\"2700\">Balloon - Bouquet</Product> <Quantity>1.0</Quantity> <ExtendedPrice>25.0</ExtendedPrice> <InputTotalTax>2.22</InputTotalTax> <Taxes taxResult=\"TAXABLE\" taxType=\"SELLER_USE\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\" taxStructure=\"SINGLE_RATE\"><Jurisdiction jurisdictionLevel=\"STATE\" jurisdictionId=\"24354\">NEW YORK</Jurisdiction> <CalculatedTax>1.0</CalculatedTax> <EffectiveRate>0.04</EffectiveRate> <Taxable>25.0</Taxable> <Imposition impositionId=\"1\">Sales and Compensating Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>72991</TaxRuleId> <InclusionRuleId>1293080</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"TAXABLE\" taxType=\"SELLER_USE\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\" taxStructure=\"SINGLE_RATE\"><Jurisdiction jurisdictionLevel=\"CITY\" jurisdictionId=\"25353\">NEW YORK</Jurisdiction> <CalculatedTax>1.13</CalculatedTax> <EffectiveRate>0.045</EffectiveRate> <Taxable>25.0</Taxable> <Imposition impositionId=\"1\">Local Sales and Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>259039</TaxRuleId> <InclusionRuleId>1293087</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"TAXABLE\" taxType=\"SELLER_USE\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\" taxStructure=\"SINGLE_RATE\"><Jurisdiction jurisdictionLevel=\"DISTRICT\" jurisdictionId=\"79774\"><![CDATA[METROPOLITAN COMMUTER TRANSPORTATION DISTRICT]]></Jurisdiction> <CalculatedTax>0.09</CalculatedTax> <EffectiveRate>0.00375</EffectiveRate> <Taxable>25.0</Taxable> <Imposition impositionId=\"1\"><![CDATA[Metropolitan Commuter Transportation District]]></Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>75714</TaxRuleId> <InclusionRuleId>1293725</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <TotalTax>2.22</TotalTax> </LineItem> <LineItem lineItemId=\"17561\" lineItemNumber=\"3\"><Product productClass=\"1600\">Kraft Mac and Cheese</Product> <Quantity>1.0</Quantity> <ExtendedPrice>4.99</ExtendedPrice> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"STATE\" jurisdictionId=\"24354\">NEW YORK</Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>4.99</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\">Sales and Compensating Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>13504</TaxRuleId> <InclusionRuleId>1293080</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"CITY\" jurisdictionId=\"25353\">NEW YORK</Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>4.99</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\">Local Sales and Use Tax</Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>259039</TaxRuleId> <InclusionRuleId>1293087</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <Taxes taxResult=\"NONTAXABLE\" taxType=\"SELLER_USE\" reasonCode=\"0834\" situs=\"DESTINATION\" taxCollectedFromParty=\"BUYER\"><Jurisdiction jurisdictionLevel=\"DISTRICT\" jurisdictionId=\"79774\"><![CDATA[METROPOLITAN COMMUTER TRANSPORTATION DISTRICT]]></Jurisdiction> <CalculatedTax>0.0</CalculatedTax> <EffectiveRate>0.0</EffectiveRate> <NonTaxable>4.99</NonTaxable> <Taxable>0.0</Taxable> <Imposition impositionId=\"1\"><![CDATA[Metropolitan Commuter Transportation District]]></Imposition> <ImpositionType impositionTypeId=\"1\">General Sales and Use Tax</ImpositionType> <TaxRuleId>75714</TaxRuleId> <InclusionRuleId>1293725</InclusionRuleId> <SummaryInvoiceText>44571098</SummaryInvoiceText> <AssistedParameters><AssistedParameter paramName=\"tax.summaryInvoiceText\" phase=\"POST\" ruleName=\"To Account - General Sales/Use Tax\" originalValue=\"\">44571098</AssistedParameter> </AssistedParameters> </Taxes> <TotalTax>0.0</TotalTax> </LineItem> </DistributeTaxResponse> <ApplicationData><ResponseTimeMS>196.6</ResponseTimeMS> </ApplicationData> </VertexEnvelope></soapenv:Body></soapenv:Envelope>";
            decimal tax = 0;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);

            XmlNodeList nodeitem = xmlDoc.GetElementsByTagName("DistributeTaxResponse");

            List<ReportHelper> reportHelpers = new List<ReportHelper>();

            foreach (XmlNode xmlnode in nodeitem)
            {
                for (int i = 0; i < xmlnode.ChildNodes.Count; i++)
                {

                    XmlNode childnode = xmlnode.ChildNodes[i];

                    System.Diagnostics.Debug.WriteLine(childnode.Name);

                    if (childnode.Name == "LineItem")
                    {

                        int productId = int.Parse(childnode.Attributes["lineItemId"].Value);

                        foreach (XmlNode lineItemNode in childnode.ChildNodes)
                        {
                            if (lineItemNode.Name == "Taxes")
                            {
                                XmlNode taxNode = lineItemNode;
                                decimal calculatedTax = Convert.ToDecimal(taxNode["CalculatedTax"].InnerText);

                                var assistedParameter = taxNode["AssistedParameters"].ChildNodes[0];
                                string ruleName = assistedParameter.Attributes["ruleName"].Value;
                                string glcode = assistedParameter.InnerText;


                                bool glfound = false;


                                foreach (ReportHelper helper in reportHelpers)
                                {
                                    if (helper.Name == glcode && helper.productId == productId)
                                    {
                                        glfound = true;
                                        helper.Total += calculatedTax;
                                        break;
                                    }
                                }

                                if (!glfound)
                                {
                                    reportHelpers.Add(new ReportHelper(productId, glcode, ruleName, calculatedTax));
                                }



                            }
                        }

                    }
                }
            }

        }


        //public static void RunFSUAR()
        //{

        //    log4net.Config.XmlConfigurator.Configure();

        //    List<MealPlan> mealplans = getMealPlanDatabaseResults();
        //    Logger.Info("==============================================================");
        //    Logger.Info("Processing " + mealplans.Count + " Meal Plans");

        //    if (mealplans.Count == 0)
        //    {
        //        return;
        //    }


        //    IList<FsuMealPlanRecord> list = new List<FsuMealPlanRecord>();
        //    List<int> ids = new List<int>();

        //    string priceAttributeNameToLookFor = "Price: $";

        //    foreach (MealPlan plan in mealplans)
        //    {
        //        string attribute = plan.Attributes;

        //        if (attribute.Contains(priceAttributeNameToLookFor))
        //        {
        //            int pFrom = attribute.IndexOf(priceAttributeNameToLookFor) + priceAttributeNameToLookFor.Length;
        //            int pTo = attribute.IndexOf("<br />", pFrom);

        //            if (pTo == -1)
        //            {
        //                //no <BR /> after the price, must be end of the string
        //                pTo = attribute.Length;
        //            }
        //            string empId = plan.RecipientAcctNum.Substring(0, plan.RecipientAcctNum.Length < 10 ? plan.RecipientAcctNum.Length : 10);
        //            string mealplanAmount = attribute.Substring(pFrom, pTo - pFrom);
        //            string orderNumber = plan.OrderItemId + "M" + plan.OrderId;

        //            /* if (plan.CategoryName == "Student Fall/Spring") //need to add 2 records - one for fall this year, one for spring next year
        //             {
        //                 var fallRecord = new FsuMealPlanRecord();
        //                 fallRecord.EmplId = empId;
        //                 fallRecord.Amount = Convert.ToString(mealplanAmount);
        //                 fallRecord.ReferenceNumber = orderNumber;
        //                 var planTerm = GetPlanTerm("Fall");
        //                 var dueDate = GetPlanDueDate("Fall 2019");
        //                 fallRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                 fallRecord.DueDate = dueDate;
        //                 list.Add(fallRecord);


        //                 var springRecord = new FsuMealPlanRecord();
        //                 springRecord.EmplId = empId;
        //                 springRecord.Amount = Convert.ToString(mealplanAmount);
        //                 springRecord.ReferenceNumber = orderNumber;

        //                 planTerm = GetPlanTerm("Spring");
        //                 dueDate = GetPlanDueDate("Spring 2020");
        //                 springRecord.Term = "2" + DateTime.Now.AddYears(1).ToString("yy") + planTerm;
        //                 springRecord.DueDate = dueDate;
        //                 list.Add(springRecord);

        //             }

        //             if (plan.CategoryName == "Student Fall 2018/Spring 2019") //need to add 2 records - one for fall this year, one for spring next year
        //             {
        //                 var fallRecord = new FsuMealPlanRecord();
        //                 fallRecord.EmplId = empId;
        //                 fallRecord.Amount = Convert.ToString(mealplanAmount);
        //                 fallRecord.ReferenceNumber = orderNumber;
        //                 var planTerm = GetPlanTerm("Fall");
        //                 var dueDate = GetPlanDueDate("Fall 2018");
        //                 fallRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                 fallRecord.DueDate = dueDate;
        //                 list.Add(fallRecord);


        //                 var springRecord = new FsuMealPlanRecord();
        //                 springRecord.EmplId = empId;
        //                 springRecord.Amount = Convert.ToString(mealplanAmount);
        //                 springRecord.ReferenceNumber = orderNumber;

        //                 planTerm = GetPlanTerm("Spring");
        //                 dueDate = GetPlanDueDate("Spring 2019");
        //                 springRecord.Term = "2" + DateTime.Now.AddYears(1).ToString("yy") + planTerm;
        //                 springRecord.DueDate = dueDate;
        //                 list.Add(springRecord);

        //             }
        //             if (plan.CategoryName == "Student Fall 2019/Spring 2020") //need to add 2 records - one for fall this year, one for spring next year
        //             {
        //                 var fallRecord = new FsuMealPlanRecord();
        //                 fallRecord.EmplId = empId;
        //                 fallRecord.Amount = Convert.ToString(mealplanAmount);
        //                 fallRecord.ReferenceNumber = orderNumber;
        //                 var planTerm = GetPlanTerm("Fall");
        //                 var dueDate = GetPlanDueDate("Fall 2019");
        //                 fallRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                 fallRecord.DueDate = dueDate;
        //                 list.Add(fallRecord);


        //                 var springRecord = new FsuMealPlanRecord();
        //                 springRecord.EmplId = empId;
        //                 springRecord.Amount = Convert.ToString(mealplanAmount);
        //                 springRecord.ReferenceNumber = orderNumber;

        //                 planTerm = GetPlanTerm("Spring");
        //                 dueDate = GetPlanDueDate("Spring 2020");
        //                 springRecord.Term = "2" + DateTime.Now.AddYears(1).ToString("yy") + planTerm;
        //                 springRecord.DueDate = dueDate;
        //                 list.Add(springRecord);

        //             }*/
        //            if (plan.CategoryName == "Student Fall/Spring") //"Student Fall 2020/Spring 2021") need to add 2 records - one for fall this year, one for spring next year
        //            {
        //                var fallRecord = new FsuMealPlanRecord();
        //                fallRecord.EmplId = empId;
        //                fallRecord.Amount = Convert.ToString(mealplanAmount);
        //                fallRecord.ReferenceNumber = orderNumber;
        //                var planTerm = GetPlanTerm("Fall");
        //                var dueDate = GetPlanDueDate("Fall 2019");
        //                fallRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                fallRecord.DueDate = dueDate;
        //                list.Add(fallRecord);


        //                var springRecord = new FsuMealPlanRecord();
        //                springRecord.EmplId = empId;
        //                springRecord.Amount = Convert.ToString(mealplanAmount);
        //                springRecord.ReferenceNumber = orderNumber;

        //                planTerm = GetPlanTerm("Spring");
        //                dueDate = GetPlanDueDate("Spring 2020");
        //                springRecord.Term = "2" + DateTime.Now.AddYears(1).ToString("yy") + planTerm;
        //                springRecord.DueDate = dueDate;
        //                list.Add(springRecord);

        //            }
        //            else if (plan.CategoryName == "Student Spring 2019") //need to add 1 record only for spring.
        //            {
        //                var springRecord = new FsuMealPlanRecord();
        //                springRecord.EmplId = empId;
        //                springRecord.Amount = Convert.ToString(mealplanAmount);
        //                springRecord.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm("Spring");
        //                var dueDate = GetPlanDueDate("Spring 2019");
        //                springRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                springRecord.DueDate = dueDate;
        //                list.Add(springRecord);

        //            }
        //            else if (plan.CategoryName == "Student Spring 2020") //need to add 1 record only for spring.
        //            {
        //                var springRecord = new FsuMealPlanRecord();
        //                springRecord.EmplId = empId;
        //                springRecord.Amount = Convert.ToString(mealplanAmount);
        //                springRecord.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm("Spring");
        //                var dueDate = GetPlanDueDate("Spring 2020");
        //                springRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                springRecord.DueDate = dueDate;
        //                list.Add(springRecord);

        //            }
        //            else if (plan.CategoryName == "Student Spring") //need to add 1 record only for spring.
        //            {
        //                var springRecord = new FsuMealPlanRecord();
        //                springRecord.EmplId = empId;
        //                springRecord.Amount = Convert.ToString(mealplanAmount);
        //                springRecord.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm("Spring");
        //                var dueDate = GetPlanDueDate("Spring");
        //                springRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                springRecord.DueDate = dueDate;
        //                list.Add(springRecord);

        //            }
        //            else if (plan.CategoryName == "Student Summer A") //need to add 1 record only for summer.
        //            {
        //                var record = new FsuMealPlanRecord();
        //                record.EmplId = empId;
        //                record.Amount = Convert.ToString(mealplanAmount);
        //                record.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm(plan.CategoryName);
        //                var dueDate = GetPlanDueDate(plan.CategoryName);
        //                record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                record.DueDate = dueDate;
        //                list.Add(record);

        //            }
        //            else if (plan.CategoryName == "Student Summer B") //need to add 1 record only for summer.
        //            {
        //                var record = new FsuMealPlanRecord();
        //                record.EmplId = empId;
        //                record.Amount = Convert.ToString(mealplanAmount);
        //                record.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm(plan.CategoryName);
        //                var dueDate = GetPlanDueDate(plan.CategoryName);
        //                record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                record.DueDate = dueDate;
        //                list.Add(record);

        //            }
        //            else if (plan.CategoryName == "Student Summer C") //need to add 1 record only for summer  .
        //            {
        //                var record = new FsuMealPlanRecord();
        //                record.EmplId = empId;
        //                record.Amount = Convert.ToString(mealplanAmount);
        //                record.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm(plan.CategoryName);
        //                var dueDate = GetPlanDueDate(plan.CategoryName);
        //                record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                record.DueDate = dueDate;
        //                list.Add(record);

        //            }
        //            else if (plan.CategoryName == "Student Summer F") //need to add 1 record only for summer.
        //            {
        //                var record = new FsuMealPlanRecord();
        //                record.EmplId = empId;
        //                record.Amount = Convert.ToString(mealplanAmount);
        //                record.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm(plan.CategoryName);
        //                var dueDate = GetPlanDueDate(plan.CategoryName);
        //                record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                record.DueDate = dueDate;
        //                list.Add(record);

        //            }

        //            else //Summer
        //            {

        //                var record = new FsuMealPlanRecord();
        //                record.EmplId = empId;
        //                record.Amount = Convert.ToString(mealplanAmount);
        //                record.ReferenceNumber = orderNumber;

        //                var planTerm = GetPlanTerm(plan.CategoryName);
        //                var dueDate = GetPlanDueDate(plan.CategoryName);
        //                record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
        //                record.DueDate = dueDate;
        //                list.Add(record);
        //            }
        //            ids.Add(plan.Id);
        //        }
        //    }


        //    decimal trailerTotal = Convert.ToDecimal(0.00) + list.Sum(rec => Convert.ToDecimal(rec.Amount));
        //    string totalTrailer = trailerTotal.ToString("{0:#,##0.00}");
        //    var factory = new FixedLengthFileEngineFactory();

        //    var path = "";
        //    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TempFolder"]))
        //    {
        //        path = ConfigurationManager.AppSettings["TempFolder"];
        //    }
        //    var fileName = "file.txt";

        //    using (var stream = File.Create(path + fileName))
        //    {
        //        var recordlayout = new FsuFixedRecordLayout();
        //        var flatFile = factory.GetEngine(recordlayout);
        //        flatFile.Write<FsuMealPlanRecord>(stream, list);
        //        var trailerlayout = new FsuFixedTrailerLayout();
        //        var flatFile2 = factory.GetEngine(trailerlayout);

        //        IList<FsuMealPlanTrailer> trailers = new List<FsuMealPlanTrailer>();
        //        var trailer = new FsuMealPlanTrailer();
        //        trailer.ControlAmount = trailerTotal;
        //        trailer.ControlCount = list.Count;
        //        trailers.Add(trailer);

        //        flatFile2.Write<FsuMealPlanTrailer>(stream, trailers);
        //        Logger.Info("Built File: " + path + fileName);
        //    }

        //    bool fileAlreadyOnServer = false;
        //    bool uploadToServer = true;
        //    bool processMealPlanRecords = true;

        //    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["ProcessMealPlanRecords"])) //app config override
        //    {
        //        processMealPlanRecords = Boolean.Parse(ConfigurationManager.AppSettings["ProcessMealPlanRecords"]);
        //    }

        //    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["UploadToServer"])) //app config override
        //    {
        //        uploadToServer = Boolean.Parse(ConfigurationManager.AppSettings["UploadToServer"]);
        //    }

        //    if (uploadToServer)
        //    {
        //        Logger.Info("uploading to server");
        //        using (var ssh = new SFTPUtil("OBS-FS02.bfs.fsu.edu", "Sodexo_MP", "F$U_sdx17#"))
        //        {
        //            IList<ChannelSftp.LsEntry> files = ssh.ListFiles("/");
        //            if (files.Count > 0)
        //            {
        //                fileAlreadyOnServer = true; //a file is still out there
        //                Logger.Info("file already on server");
        //            }
        //            else
        //            {
        //                Logger.Info("no file on server");
        //                fileAlreadyOnServer = false;
        //            }


        //            if (!fileAlreadyOnServer)
        //            {
        //                Logger.Info("Uploading To Server: " + path + fileName);
        //                ssh.PutFile(path + fileName, "SDXMP.dat");

        //                if (processMealPlanRecords)
        //                {
        //                    Logger.Info("Processing Meal Plans");
        //                    SetToProcessed(ids);
        //                }
        //                Logger.Info("Backing up file");
        //                CreateFileBackup(path, fileName);
        //            }
        //        }
        //    }
        //    Logger.Info("Process Complete");
        //    Logger.Info("==============================================================");
        //}


        public static string GetPlanDueDate(string description)
        {

            if (description.Contains("Summer A"))
            {
                return "20190524";
            }
            else if (description.Contains("Summer B"))
            {
                return "20190524";
            }
            else if (description.Contains("Summer C"))
            {
                return "20190708";
            }
            else if (description.Contains("Summer F"))
            {
                return "20190525";
            }
            else if (description.Contains("Summer"))
            {
                return "20190524";
            }
            else if (description.Contains("Fall 2018"))
            {
                return "20180907";
            }
            else if (description.Contains("Fall 2019"))
            {
                return "20190906";
            }
            else if (description.Contains("Fall 2020"))
            {
                return "20200907";
            }
            else if (description.Contains("Spring 2019"))
            {
                return "20190118";
            }
            else if (description.Contains("Spring 2020"))
            {
                return "20200117";
            }
            else if (description.Contains("Fall"))
            {
                return "20190906";
            }
            else if (description.Contains("Spring"))
            {
                return "20200119";
            }

            return "20180525"; //SUMMER;
        }


        public static string GetPlanTerm(string description)
        {
            if (description.Contains("Summer"))
            {
                return "6";
            }
            else if (description.Contains("Fall"))
            {
                return "9";
            }
            else if (description.Contains("Spring"))
            {
                return "1";
            }

            return "9"; //FALL
        }


        private static void CreateFileBackup(string path, string fileToCopy)
        {
            string path2 = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + "-PROCESSED-" + fileToCopy;

            try
            {
                // Copy the file.
                File.Copy(path + fileToCopy, path + path2);

                Logger.Info(String.Format("{0} copied to {1}", path, path2));

                File.Delete(path + fileToCopy);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }


      //  private static List<MealPlan> getMealPlanDatabaseResults()
      //  {

      //      var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
      //      var reportList = new List<MealPlan>();
      //      using (var conn = new SqlConnection(connStr))
      //      using (var cmd = conn.CreateCommand())
      //      {
      //          cmd.CommandType = CommandType.Text;
      //          var sqlSelect = string.Format(@"
      //                 select 
						// mp.*, 
						// AttributeDescription, 
						// oi.PriceExclTax, 
						// o.id as 'OrderId', 
						// oi.id as 'OrderSiteProductId', 
						// p.Name as 'ProductName', 
						// c.name as 'CategoryName' 
						//from MealPlan mp
      //                  inner join OrderItem oi 
      //                  on oi.Id = mp.PurchasedWithOrderItemId
      //                  inner join Product p
      //                  on p.Id = oi.productid
      //                  inner join Product_Category_Mapping pcm
      //                  on pcm.ProductId = p.Id
      //                  inner join Category c
      //                  on c.Id = pcm.CategoryId
      //                  inner join [order] o 
      //                  on o.Id = oi.OrderId
      //                  where IsProcessed = 0
      //                  --where o.createdonutc between '2019-02-05 00:00:01' AND '2019-02-07 23:59:59'  
      //                  --where processeddate between '2018-12-21 00:00:01' AND '2018-12-21 23:59:59'  
      //                  and  o.storeid = 318
      //                  and oi.PriceInclTax = 0.0000");
      //          cmd.CommandText = sqlSelect;
      //          conn.Open();
      //          using (var reader = cmd.ExecuteReader())
      //          {
      //              while (reader.Read())
      //              {
      //                  var mealPlan = new MealPlan();
      //                  mealPlan.Id = ((int)reader["Id"]);
      //                  mealPlan.RecipientAcctNum = ((string)reader["RecipientAcctNum"]).Trim();
      //                  mealPlan.Attributes = ((string)reader["AttributeDescription"]).Trim();
      //                  mealPlan.OrderId = ((int)reader["OrderId"]);
      //                  mealPlan.OrderItemId = ((int)reader["OrderSiteProductId"]);
      //                  mealPlan.PlanName = ((string)reader["ProductName"]);
      //                  mealPlan.CategoryName = ((string)reader["CategoryName"]);

      //                  //     mealPlan.Amount = (decimal)reader["PriceExclTax"];
      //                  /*  res.UnitId = ((string)reader["Unit Id"]).Trim();
      //                    res.UnitName = ((string)reader["Unit Name"]).Trim();
      //                    res.MasterCataLogger.ategory = ((string)reader["Master CataLogger.Category"]).Trim();
      //                    res.MasterCataLogger.roductId = (int)reader["Master CataLogger.Product ID"];
      //                    res.MasterCataLogger.roductName = ((string)reader["Master CataLogger.Product Name"]).Trim();
      //                    res.UniversalPlu = DBNull.Value.Equals(reader["Master CataLogger.Product SKU"]) ? "" : ((string)reader["Master CataLogger.Product SKU"]).Trim();
      //                    res.LocalProductName = ((string)reader["Local Product Name"]).Trim();
      //                    res.LocalPrice = decimal.Round((decimal)reader["Local Price"], 2);
      //                    res.TotalSalesExclTax = decimal.Round((decimal)reader["Total Sales Excl Tax"], 2);
      //                    res.TotalSalesInclTax = decimal.Round((decimal)reader["Total Sales Incl Tax"], 2);
      //                    res.QtySold = (int)reader["Qty Sold"];
      //                    res.IsPublished = ((string)reader["Is Published"]).Trim();
      //                    res.LocalProductDetailUrl = ((string)reader["View Local Product"]).Trim();*/

      //                  reportList.Add(mealPlan);
      //              }
      //          }
      //      }
      //      return reportList;
      //  }


        private static void SetToProcessed(List<int> ids)
        {
            StringBuilder queryIds = new StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                queryIds.Append(ids[i] + (i + 1 == ids.Count ? "" : ","));
            }

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                      update mealplan set IsProcessed = 1, ProcessedDate = '{0}' where id in({1})", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), queryIds);
                cmd.CommandText = sqlSelect;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


    }
}
