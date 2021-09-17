import threading
from random import randint
from time import sleep, time


n = 5
customers_count = 2 * n                         # number of customers
customers_left = 0                              # number of finished customers
get_sleep = True
barber_end = True
waiting_room = threading.Semaphore(n)            # capacity of waiting room
barber_chair = threading.Semaphore(1)
barber_works = threading.Semaphore(0)



def barber_shop(id):
    global customers_count, barber_end, customers_left, waiting_room, barber_chair

    waiting_room.acquire()                  # customer enters only if there is a free chair
    print("Customer", id, "is sitting in the waiting room.")
    barber_chair.acquire()                  # customer waiting for free barber chair
    waiting_room.release()
    get_hair_cut(id)                        # customer's turn
    barber_chair.release()
    
        # rest of function is only for stop program and printing info
    print("Customer", id, "left the barber shop.")
    customers_left += 1
    if customers_left == customers_count:
        print("All customers left the barber shop.")
        barber_end = False
    

def get_hair_cut(id):
    global get_sleep, barber_works

    print("Customer", id, "is waiting for the barber.")
    get_sleep = False                       # customer wakes up the barber
    barber_works.acquire()
    print("Barber is working on customer", id)
    sleep(2)


def cut_hair():
    global get_sleep, customers_count, barber_end, barber_works

    while barber_end:
        time_out = 0
        while get_sleep and barber_end:     # barber is sleeping
            sleep(1)
            time_out += 1
            #print("Barber is sleeping.)
        barber_works.release()
        get_sleep = True
    print("No more customers. Barber went to pub.")


barber = threading.Thread(target=cut_hair)
barber.start()

for id in range(customers_count):
    customer = threading.Thread(target=barber_shop, args=(id, ))
    customer.start()
    sleep(randint(1, 3))

