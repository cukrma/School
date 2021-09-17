import threading


class Labour:
    def __init__(self):
        self.lock = threading.Lock()
        self.monitor = threading.Condition(self.lock)
        self.func_ready = False
        self.func = None
        self.futures = []

    def _work(self):
        with self.lock:
            while True:
                while not self.func_ready:
                    self.monitor.wait()
                self.func_ready = False
                self.futures[0].result = self.func()
                self.futures[0].sem.release()
                self.futures.pop(0)

    def set_worker_count(self, n):
        for i in range(n):
            thread = threading.Thread(target=self._work)
            thread.start()

    def compute(self, fun):
        with self.lock:
            self.func = fun
            self.func_ready = True
            self.monitor.notify()
            f = Future()
            self.futures.append(f)
            return f


class Future:
    def __init__(self):
        self.sem = threading.Semaphore(0)
        
    def deref(self):
        self.sem.acquire()
        return self.result


def factorial(n):
    if n == 0:
        return 1
    else:
        return n * factorial(n - 1)

def fibonacci(n):
    if n <= 1:
        return n
    else:
        return fibonacci(n - 1) + fibonacci(n - 2)

def worker_function():
    return factorial(950)


labour = Labour()
labour.set_worker_count(5)

factorial_future = labour.compute(worker_function)
large_factorial = factorial_future.deref()
print("large_factorial ended")

fibonacci_future = labour.compute(lambda: fibonacci(30))
large_fibonacci = fibonacci_future.deref()
print("large_fibonacci ended")

