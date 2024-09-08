namespace NewFace.Common.Constants;

public static class MessageCode
{
    public static class HttpStatus
    {
        public const string SUCCESS = "200";
        public const string BAD_REQUEST = "400";
        public const string UNAUTHORIZED = "401";
        public const string FORBIDDEN = "403";
        public const string NOT_FOUND = "404";
        public const string METHOD_NOT_ALLOWED = "405";
        public const string NOT_ACCEPTABLE = "406";
        public const string UNSUPPORTED_MEDIA_TYPE = "415";
        public const string INTERNAL_SERVER_ERROR = "500";
    }

    public enum Custom
    {
        NOT_REGISTERED_USER = 1001,
        REGISTERED_EMAIL = 1002,
        INVALID_TOKEN = 1003,
        DELETED_USER = 1004,
        NOT_FOUND_USER = 2001,
        INVALID_ROLE = 2002,
        REGISTERED_ROLE = 2003,
        USER_NOT_ACTOR = 2004,
        INVALID_MOBILE_NUMBER = 2008,
        SMS_SEND_FAILED = 3003,
        OTP_NOT_FOUND = 3004,
        OTP_MISMATCH = 3005,
        NOT_AUTH_MOBILE = 3006,
        INVALID_FILE_TYPE = 4001,
        UNKNOWN_ERROR = 9999
    }

    public static readonly Dictionary<Custom, string> CustomMessages = new Dictionary<Custom, string>
    {
        { Custom.NOT_REGISTERED_USER, "등록되지 않은 사용자 입니다." },
        { Custom.REGISTERED_EMAIL, "이미 등록된 이메일 입니다." },
        { Custom.INVALID_TOKEN, "토큰 정보가 유효하지 않습니다." },
        { Custom.DELETED_USER, "회원 탈퇴한 계정입니다. 관리자에게 문의하세요." },
        { Custom.NOT_FOUND_USER, "사용자 정보를 찾을 수 없습니다." },
        { Custom.INVALID_ROLE, "존재하지 않는 Role입니다." },
        { Custom.REGISTERED_ROLE, "이미 등록된 Role입니다." },
        { Custom.USER_NOT_ACTOR, "Actor Role을 가지고 있지 않습니다." },
        { Custom.INVALID_MOBILE_NUMBER, "유효하지 않은 전화 번호 입니다." },
        { Custom.SMS_SEND_FAILED, "SMS 전송에 실패 했습니다." },
        { Custom.OTP_NOT_FOUND, "OTP Code가 만료되었거나 존재하지 않습니다." },
        { Custom.OTP_MISMATCH, "OTP Code가 틀립니다." },
        { Custom.NOT_AUTH_MOBILE, "OTP 인증이 되어 있지 않은 상태입니다" },
        { Custom.INVALID_FILE_TYPE, "허용되지 않는 파일 형식입니다." },
        { Custom.UNKNOWN_ERROR, "알수 없는 에러 입니다. 관리자에게 문의하세요." }
    };
}