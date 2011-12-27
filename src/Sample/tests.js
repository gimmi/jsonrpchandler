describe("Jasmine", function () {
	var rpc;

	beforeEach(function () {
		rpc = new JsonRpc('rpc');
	});

	it('should echo string value', function () {
		var actual;
		runs(function () {
			rpc.call('Service.echo', 'Hello world', function (ret, success) {
				this.success = success;
				this.done = true;
				actual = ret;
			}, this);
		});
		
		waitsFor(function () {
			return this.done;
		}, 'Server call', 1000);
		
		runs(function () {
			expect(actual).toEqual('Hello world');
			expect(this.success).toBeTruthy();
		});
	});

});
