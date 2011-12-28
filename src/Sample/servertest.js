/*global describe, beforeEach, expect, it */

describe('server test', function () {
	var target;

	beforeEach(function () {
		target = new JsonRpc('rpc');
	});

	it('should echo string value', function () {
		var actual;

		runs(function () {
			target.call('stringEcho', 'Hello world', function (ret) {
				actual = ret;
			}, this);
		});

		waitsFor(function () {
			return !!actual;
		}, 'Server call', 1000);

		runs(function () {
			expect(actual).toEqual('Hello world');
		});
	});

	it("should not corrupt strings", function () {
		runs(function () {
			target.call('stringEcho', 'òàùèé', function (result) {
				this.actual = result;
				this.completed = true;
			}, this);
		});

		waitsFor(function () {
			return this.completed;
		}, 'Server call', 1000);

		runs(function () {
			expect(this.actual).toEqual('òàùèé');
		});
	});

	it('should echo numeric value', function () {
		var actual;
		runs(function () {
			target.call('numberEcho', 3.14, function (result) {
				this.done = true;
				actual = result;
			}, this);
		});
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		runs(function () {
			expect(actual).toEqual(3.14);
		});
	});

	it('should echo bool value', function () {
		var actual;
		runs(function () {
			target.call('boolEcho', true, function (result) {
				this.done = true;
				actual = result;
			}, this);
		});
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		runs(function () {
			expect(actual).toEqual(true);
		});
	});

	it('should echo array', function () {
		var actual;
		runs(function () {
			target.call('arrayEcho', [1, 2, 3], function (result) {

				this.done = true;
				actual = result;
			}, this);
		});
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		runs(function () {

			expect(actual).toEqual([1, 2, 3]);
		});
	});

	it('should echo object', function () {
		var actual;
		var obj = { stringValue: 'hello', numberValue: 3.14, boolValue: true };
		runs(function () {
			target.call('objectEcho', obj, function (result) {

				this.done = true;
				actual = result;
			}, this);
		});
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		runs(function () {

			expect(actual).toEqual(obj);
		});
	});

	it('should echo raw JSON object', function () {
		var actual;
		var obj = { stringValue: 'hello', numberValue: 3.14, boolValue: true };
		runs(function () {
			target.call('jObjectEcho', obj, function (result) {

				this.done = true;
				actual = result;
			}, this);
		});
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		runs(function () {

			expect(actual).toEqual(obj);
		});
	});

	it("should call methods without parameters", function () {
		runs(function () {
			target.call('noParams', function (result) {

				this.actual = result;
				this.done = true;
			}, this);
		});

		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);

		runs(function () {

			expect(this.actual).toEqual(true);
		});
	});

	it("should notify exception as expected", function () {
		var successArgs, failureArgs, callbackArgs;

		runs(function () {
			target.call('exception', {
				success: function () {
					successArgs = arguments;
				},
				failure: function () {
					failureArgs = arguments;
				},
				callback: function () {
					callbackArgs = arguments;
				}
			});
		});

		waitsFor(function () {
			return !!callbackArgs;
		}, 'Server call', 1000);

		runs(function () {
			expect(successArgs).toBeUndefined();
			expect(failureArgs).toEqual(['An error occured']);
			expect(callbackArgs).toEqual([false, 'An error occured']);
		});
	});
});