describe('server test', function () {
	var target;

	beforeEach(function () {
		target = new JsonRpc('rpc');
	});

	it('should echo string value', function () {
		var actual;
		
		runs(function () {
			target.call('Service.echo', 'Hello world', function (ret) {
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
})
