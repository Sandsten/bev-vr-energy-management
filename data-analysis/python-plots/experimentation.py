import matplotlib.pyplot as plt
import numpy as np

# Create range of numbers 0 to 49
rng = np.arange(50)
# Create random numbers between 0 and 10 in a 3,50 matrix
rnd = np.random.randint(0, 10, size=(3, rng.size))
# create an array of increasing years
yrs = 1950 + rng

# Create a plot with the size 5,3 containing one set of axes
fig, ax  = plt.subplots(figsize=(5,3))

# Modify the created 
ax.stackplot(yrs, rng+rnd, labels=['Eastasia', 'Eurasia', 'Oceana'])
ax.set_title("Combined debt growth over time")
ax.legend(loc="upper left")
ax.set_ylabel("Total debt")
ax.set_xlim(xmin=yrs[0], xmax=yrs[-1])

# Cleans up whitespace padding
fig.tight_layout()


