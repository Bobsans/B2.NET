using B2.Models;

namespace B2.Test; 

public class BaseTest {
	protected B2Options Options { get; set; }

	// TODO: Change these to valid keys to run tests
	protected const string APPLICATION_KEY = "K004CxuP/q5q6YSfVm9Nz/CR/pA/lyg";
	protected const string APPLICATION_KEY_ID = "00475377ae840cb0000000001";

	protected const string RESTRICTED_APPLICATION_KEY = "K0046+wCsbYFFgmJWZ1VGTC2YUFnYYA";
	protected const string RESTRICTED_APPLICATION_KEY_ID = "00475377ae840cb0000000002";

	protected BaseTest() {
		Options = new B2Options {
			KeyId = APPLICATION_KEY_ID,
			ApplicationKey = APPLICATION_KEY
		};
	}
}
