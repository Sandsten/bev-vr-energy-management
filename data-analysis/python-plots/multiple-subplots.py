# -*- coding: utf-8 -*-
"""
Created on Thu Sep 17 11:21:35 2020

@author: HTC Vive
"""

import matplotlib.pyplot as plt
import numpy as np

x = np.random.randint(low=1, high=11, size=50)
y = x + np.random.randint(low=1, high=5, size=x.size)
data = np.column_stack((x,y))

# nrows and ncols is the set of axis, in this case we have one row with two axes
fig, (ax1, ax2) = plt.subplots(nrows=1, ncols=2, figsize=(8,4))

# fig.axes[0] is ax1

ax1.scatter(x=x, y=y, marker="o", c="r", edgecolor="b")
ax1.set_title("Scatter: $x$ vs $y$")
ax1.set_ylabel("$y$")
ax1.set_xlabel("$x$")

ax2.hist(data, bins=np.arange(data.min(), data.max()), label=("x", "y"))
ax2.legend(loc=(0.65, 0.8))
ax2.set_title("Frequencies of $x$ and $y$")
ax2.yaxis.tick_right()