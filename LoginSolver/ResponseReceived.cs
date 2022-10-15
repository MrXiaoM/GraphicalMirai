using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginSolver
{
    public class ResponseReceived
    {
        public string requestId;
        public Response response;
    }
    public class Response
    {
        public string url;
    }
    public class ResponseBody
    {
        public string body;
        public bool base64Encoded;
        public string GetBody()
        {
            if (base64Encoded) return Encoding.UTF8.GetString(Convert.FromBase64String(body));
            return body;
        }
    }
    public class CaptchaResult
    {
        /*
         {"errorCode":"0","randstr":"@9oY",
        "ticket":"t034ceb032tex2RwACWSwQlOwAl3e_5TGQ2FOozXnE87DIAaUP7B7RIX_LR3oAs9wrxksnMxX61DiY16Cc5OPllhQZvhbqIdQNwACkikeNj-pIktft7xWr3Ig5hVqLrbADdgK-D-bdepBU*",
        "errMessage":"",
        "sess":""}
         */
        public string errorCode;
        public string randstr;
        public string ticket;
        public string errMessage;
        public string sess;
    }
}
