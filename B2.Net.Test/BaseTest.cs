using B2.Models;

namespace B2.Test; 

public class BaseTest {
	protected B2Options Options { get; set; }

	// TODO Change these to valid keys to run tests
	protected const string APPLICATION_KEY = "K0016q0BcoroQmkADj/Kne4y3ul6AWc";
	protected const string APPLICATION_KEY_ID = "00151189a8b4c7a000000000e";

	protected const string RESTRICTED_APPLICATION_KEY = "K0019m9qz095omc+WsnREy5mWsxNmtQ";
	protected const string RESTRICTED_APPLICATION_KEY_ID = "00151189a8b4c7a000000000d";

	protected BaseTest() {
		Options = new B2Options {
			KeyId = TestConstants.KeyId,
			ApplicationKey = TestConstants.ApplicationKey
		};
	}
}
