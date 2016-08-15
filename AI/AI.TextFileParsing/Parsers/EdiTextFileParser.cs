using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using AI.Common.Security;
using OopFactory.X12.Hipaa.Claims;
using OopFactory.X12.Hipaa.Claims.Services;
using OopFactory.X12.Parsing;

namespace AI.TextFileParsing
{
    public class EdiTextFileParser
    {
        private readonly IFileData _fileData;

        public EdiTextFileParser(IFileData fileData)
        {
            _fileData = fileData;
        }

        public List<string> Parse(List<Exception> exceptions)
        {
            var result = new List<string>();

            X12Parser parser = new X12Parser();
            ClaimFormTransformationService service = new ClaimFormTransformationService(
                new ProfessionalClaimToHcfa1500FormTransformation(""),
                new InstitutionalClaimToUB04ClaimFormTransformation(""),
                new DentalClaimToJ400FormTransformation(""),
                parser
            );

            try
            {
                Stream fileStream = _fileData.GetReadStream();
                ClaimDocument claimDoc = service.Transform837ToClaimDocument(fileStream);

                XmlSerializer xs = new XmlSerializer(typeof(Claim));
                foreach (Claim claim in claimDoc.Claims)
                {
                    MemoryStream ms = new MemoryStream();
                    xs.Serialize(ms, claim);
                    ms.Seek(0, SeekOrigin.Begin);
                    StreamReader sr = new StreamReader(ms);
                    string claimXml = sr.ReadToEnd();
                    result.Add(claimXml);
                    //sr.Dispose();
                    //ms.Dispose();
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                throw new Exception("Aborting import of EDI file");
            }

            return result;
        }
    }
}