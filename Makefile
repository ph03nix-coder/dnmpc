build:
	@echo Building...
	@mcs Program.cs -out:dnmpc

install:
	@echo Installing...
	@cp dnmpc ~/.local/bin/