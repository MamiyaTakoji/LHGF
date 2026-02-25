import os
for i in range(122):
    if i+1<10:
        os.rename(f"7月27日 (2)_out000{i+1}.png", f"frame_{i}.png")
    elif i+1<100:
        os.rename(f"7月27日 (2)_out00{i+1}.png", f"frame_{i}.png")
    else:
        os.rename(f"7月27日 (2)_out0{i+1}.png", f"frame_{i}.png")