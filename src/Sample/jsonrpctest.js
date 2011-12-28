﻿describe("jsonrpc", function () {
	var target;

	beforeEach(function () {
		target = new JsonRpc('rpc');
	});

	it("isFunction", function () {
		expect(target._isFunction(1)).toBe(false);
		expect(target._isFunction('a')).toBe(false);
		expect(target._isFunction(undefined)).toBe(false);
		expect(target._isFunction(null)).toBe(false);
		expect(target._isFunction(function () {
		})).toBe(true);
	});

	it('should interpret call with succes function and scope', function () {
		var scope = {};
		var successFn = jasmine.createSpy();

		var actual = target._getParams('method', 1, 2, 3, successFn, scope);

		expect(actual.request.id).toEqual(1);
		expect(actual.request.method).toEqual('method');
		expect(actual.request.params).toEqual([1, 2, 3]);
		expect(actual.success).toBe(successFn);
		expect(actual.scope).toBe(scope);
	});

	it('should interpret call with options', function () {
		var scope = {},
		    successFn = jasmine.createSpy(),
		    failureFn = jasmine.createSpy(),
		    callbackFn = jasmine.createSpy();

		var actual = target._getParams('method', 1, 2, 3, {
			success: successFn,
			failure: failureFn,
			callback: callbackFn,
			scope: scope
		});

		expect(actual.request.id).toEqual(1);
		expect(actual.request.method).toEqual('method');
		expect(actual.request.params).toEqual([1, 2, 3]);
		expect(actual.success).toBe(successFn);
		expect(actual.failure).toBe(failureFn);
		expect(actual.callback).toBe(callbackFn);
		expect(actual.scope).toBe(scope);
	});

	it('should do json post with expected parameters', function () {
		spyOn(target, '_doJsonPost');
		var scope = {};
		var successFn = jasmine.createSpy();

		target.call('method', 1, 2, 3, successFn, scope);

		expect(target._doJsonPost).toHaveBeenCalledWith('rpc', {
			jsonrpc: '2.0',
			id: 1,
			method: 'method',
			params: [1, 2, 3]
		}, jasmine.any(Function));
	});

	it('should invoke expected callbacks on succesful call', function () {
		var scope = {},
		    successFn = jasmine.createSpy(),
		    failureFn = jasmine.createSpy(),
		    callbackFn = jasmine.createSpy();

		spyOn(target, '_doJsonPost').andCallFake(function (url, data, callback) {
			callback(true, { result: 'return val' });
		});

		target.call('method', 1, 2, 3, {
			success: successFn,
			failure: failureFn,
			callback: callbackFn,
			scope: scope
		});

		expect(failureFn).not.toHaveBeenCalled();
		expect(successFn).toHaveBeenCalledWith('return val');
		expect(successFn.callCount).toBe(1);
		expect(callbackFn).toHaveBeenCalledWith(true, 'return val');
		expect(callbackFn.callCount).toBe(1);
	});

	it('should invoke expected callbacks on transport error', function () {
		var scope = {},
		    successFn = jasmine.createSpy(),
		    failureFn = jasmine.createSpy(),
		    callbackFn = jasmine.createSpy();

		spyOn(target, '_doJsonPost').andCallFake(function (url, data, callback) {
			callback(false, 'error msg');
		});

		target.call('method', 1, 2, 3, {
			success: successFn,
			failure: failureFn,
			callback: callbackFn,
			scope: scope
		});

		expect(failureFn).toHaveBeenCalledWith('error msg');
		expect(failureFn.callCount).toBe(1);
		expect(successFn).not.toHaveBeenCalled();
		expect(callbackFn).toHaveBeenCalledWith(false, 'error msg');
		expect(callbackFn.callCount).toBe(1);
	});

	it('should invoke expected callbacks on rpc error', function () {
		var scope = {},
		    successFn = jasmine.createSpy(),
		    failureFn = jasmine.createSpy(),
		    callbackFn = jasmine.createSpy();

		spyOn(target, '_doJsonPost').andCallFake(function (url, data, callback) {
			callback(true, { error: { message: 'rpc error' } });
		});

		target.call('method', 1, 2, 3, {
			success: successFn,
			failure: failureFn,
			callback: callbackFn,
			scope: scope
		});

		expect(failureFn).toHaveBeenCalledWith('rpc error');
		expect(failureFn.callCount).toBe(1);
		expect(successFn).not.toHaveBeenCalled();
		expect(callbackFn).toHaveBeenCalledWith(false, 'rpc error');
		expect(callbackFn.callCount).toBe(1);
	});
});
