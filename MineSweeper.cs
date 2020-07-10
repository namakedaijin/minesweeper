using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

class MineSweeper : Form{
	
	List<List<Block>> board = new List<List<Block>>();
	System.Timers.Timer timer = new System.Timers.Timer();
	System.Timers.Timer timeCounter = new System.Timers.Timer();
	int fieldWidth = 30;
	int fieldHeight = 16;
	int mineAmount = 99;
	int flag;
	int time = 0;
	bool playing = false;
	bool gameOver = false;
	bool gameClear = false;
	
	protected override void OnLoad(EventArgs e){
		ClientSize = new Size(30 * (fieldWidth + 1) + 5 * (fieldWidth - 1),
								30 * (fieldHeight + 1) + 5 * (fieldHeight - 1) + 60);
		Location = new Point(0, 0);
		BackColor = Color.Black;
		flag = mineAmount;
		DoubleBuffered = true;
		
		MouseDown += new MouseEventHandler(onMouseDown);
		MouseDoubleClick += new MouseEventHandler(onMouseDoubleClick);
		timer.Elapsed += new System.Timers.ElapsedEventHandler(onTimer);
		timer.Interval = 20;
		timer.Start();
		timeCounter.Elapsed += new System.Timers.ElapsedEventHandler(timeCount);
		timeCounter.Interval = 1000;
		
		createBoard();
	}

	protected override void OnPaint(PaintEventArgs e){
		Graphics g = e.Graphics;
		Font gothic = new Font("MS gothic", 18);
		SolidBrush white = new SolidBrush(Color.White);
		foreach(List<Block> bl in board){
			foreach(Block b in bl){
				b.draw(g);
			}
		}
		g.DrawString(flag.ToString(), gothic, white, 
			(30 * (fieldWidth + 1) + 5 * (fieldWidth - 1)) / 7,
			(30 * (fieldHeight + 1) + 5 * (fieldHeight - 1) + 10));
		g.DrawString(time.ToString(), gothic, white, 
			(30 * (fieldWidth + 1) + 5 * (fieldWidth - 1)) / 9 * 7,
			(30 * (fieldHeight + 1) + 5 * (fieldHeight - 1) + 10));
		if(gameOver){
			g.DrawString("Game Over", gothic, white, 
			(30 * (fieldWidth + 1) + 5 * (fieldWidth - 1)) / 2 - 60,
			(30 * (fieldHeight + 1) + 5 * (fieldHeight - 1) + 10));
		}
		if(gameClear){
			g.DrawString("Game Clear", gothic, white, 
			(30 * (fieldWidth + 1) + 5 * (fieldWidth - 1)) / 2 - 60,
			(30 * (fieldHeight + 1) + 5 * (fieldHeight - 1) + 10));
		}
	}
	
	protected override void OnKeyDown(KeyEventArgs e){
		if(e.KeyCode == Keys.Escape){
			resetBoard();
			playing = false;
			gameClear = false;
			gameOver = false;
		}
	}
	
	void timeCount(object sender, System.Timers.ElapsedEventArgs e){
		time++;
	}
	
	void onTimer(object sender, System.Timers.ElapsedEventArgs e){
		int openCnt = 0;
		int flagCnt = 0;
		foreach(List<Block> bl in board){
			foreach(Block b in bl){
				if(b.open){
					openCnt++;
					if(b.mine){
						foreach(List<Block> bl1 in board){
							foreach(Block b1 in bl1){
								b1.openBlock();
								b1.clickDisable();
								gameOver = true;
								playing = false;
							}
						}
					}
				}
				if(b.flag){
					flagCnt++;
				}
			}
		}
		if((openCnt == (fieldWidth * fieldHeight - mineAmount)) && !gameOver){
			foreach(List<Block> bl in board){
				foreach(Block b in bl){
					b.clickDisable();
				}
			}	
			gameClear = true;
			playing = false;
		}
		flag = mineAmount - flagCnt;
		if(playing){
			timeCounter.Start();
		}else{
			timeCounter.Stop();
		}
		Invalidate();
	}
	
	void onMouseDown(object sender, MouseEventArgs e){
		foreach(List<Block> bl in board){
			foreach(Block b in bl){
				if(b.isClicked(e.X, e.Y)){
					if(!playing && !gameOver && !gameClear){
						Initialize(b.mX, b.mY);
						playing = true;
					}
					if(e.Button == MouseButtons.Left){
						b.openBlock();
						if(b.number == 0){
							openRecursion(b);
						}
					}else if(e.Button == MouseButtons.Right){
						b.setFlag();
					}
				}
			}
		}
	}
	
	void onMouseDoubleClick(object sender, MouseEventArgs e){
		foreach(List<Block> bl in board){
			foreach(Block b in bl){
				if(b.isClicked(e.X, e.Y)){
					if(!playing && !gameOver && !gameClear){
						Initialize(b.mX, b.mY);
						playing = true;
					}
					int flaged = 0;
					for(int i = -1; i <= 1; i++){
						for(int j = -1; j <= 1; j++){
							if(((b.mX + j) >= 0) && ((b.mX + j) <= (fieldWidth - 1))
							&& ((b.mY + i) >= 0) && ((b.mY + i) <= (fieldHeight - 1))
							&& !((i == 0) && (j == 0))){
								if(board[b.mY + i][b.mX + j].flag){
									flaged++;
									
								}
							}
						}
					}
					if(b.number != 0 && b.open && (flaged == b.number)){
						openRecursion(b);
					}
				}
			}
		}
	}
	
	void openRecursion(Block b){
		for(int i = -1; i <= 1; i++){
			for(int j = -1; j <= 1; j++){
				if(((b.mX + j) >= 0) && ((b.mX + j) <= (fieldWidth - 1))
				&& ((b.mY + i) >= 0) && ((b.mY + i) <= (fieldHeight - 1))
				&& !((i == 0) && (j == 0))){
					Block block = board[b.mY + i][b.mX + j];
					if(!block.open){
						if(block.number == 0){
							block.openBlock();
							openRecursion(block);
						}else{
							block.openBlock();
						}
					}
				}
			}
		}
	}
	
	void Initialize(int selectedX, int selectedY){
		resetBoard();
		Random random = new Random();
		int mineCnt = 0;
		while(mineCnt != mineAmount){
			bool check = false;
			int rX = random.Next(fieldWidth);
			int rY = random.Next(fieldHeight);
			for(int i = -1; i <= 1; i++){
				for(int j = -1; j <= 1; j++){
					if((rX == selectedX + j) && (rY == selectedY + i)){
						check = true;
					}
				}
			}
			Block b = board[rY][rX];
			if(!b.mine && !check){
				b.setMine();
				mineCnt++;
				for(int i = -1; i <= 1; i++){
					for(int j = -1; j <= 1; j++){
						if(((rX + j) >= 0) && ((rX + j) <= (fieldWidth - 1))
						&& ((rY + i) >= 0) && ((rY + i) <= (fieldHeight - 1))
						&& !((i == 0) && (j == 0))){
							board[rY + i][rX + j].number++;
						}
					}
				}
			}
		}
	}
	
	void createBoard(){
		board.Clear();
		int x = 0, y = 0;
		for(int i = 30; i < 30 + 35 * fieldHeight; i += 35){
			List<Block> b = new List<Block>();
			x = 0;
			for(int j = 30; j < 30 + 35 * fieldWidth; j += 35){
				b.Add(new Block(j, i, x, y));
				x++;
			}
			board.Add(b);
			y++;
		}
	}
	
	void resetBoard(){
		time = 0;
		foreach(List<Block> bl in board){
			foreach(Block b in bl){
				b.number = 0;
				b.mine = false;
				b.open = false;
				b.flag = false;
				b.clickable = true;
			}
		}
	}

	static void Main(){
		Application.Run(new MineSweeper());
	}
}

class Block{
	public int mX, mY;
	public int number = 0;
	public bool mine = false;
	public bool open = false;
	public bool flag = false;
	public bool clickable = true;
	int mPointX, mPointY;
	int mHeight = 30, mWidth = 30;
	SolidBrush mWhite = new SolidBrush(Color.White);
	SolidBrush mRed = new SolidBrush(Color.Red);
	SolidBrush mGray = new SolidBrush(Color.FromArgb(255,80,80,80));
	SolidBrush mGreen = new SolidBrush(Color.Green);
	Font gothic = new Font("MS gothic", 16);
	
	public Block(int xPoint, int yPoint, int x, int y){
		mPointX = xPoint;
		mPointY = yPoint;
		mX = x;
		mY = y;
	}
	
	public void draw(Graphics g){
		if(!open){
			if(flag){
				g.FillRectangle(mGreen, mPointX - mWidth/2, mPointY - mHeight/2, mWidth, mHeight);
			}else{
				g.FillRectangle(mWhite, mPointX - mWidth/2, mPointY - mHeight/2, mWidth, mHeight);
			}
		}else{
			if(!mine){
				g.FillRectangle(mGray, mPointX - mWidth/2, mPointY - mHeight/2, mWidth, mHeight);
				if(number != 0){
					g.DrawString(number.ToString() , gothic, mWhite, mPointX - 8, mPointY - 8);
				}
			}else{
				g.FillRectangle(mRed, mPointX - mWidth/2, mPointY - mHeight/2, mWidth, mHeight);
			}
		}
	}
	
	public bool isClicked(int x, int y){
		return ((x >= (mPointX - mWidth/2))&&(x <= (mPointX + mWidth/2)))
		&&((y >= mPointY - mHeight/2)&&(y <= mPointY + mHeight/2));
	}
	
	public void setMine(){
		mine = true;
	}
	
	public void openBlock(){
		if(clickable){
			if(!open && !flag){
				open = true;
			}	
		}
	}
	
	public void setFlag(){
		if(clickable){	
			if(!flag && !open){
				flag = true;
			}else{
				flag = false;
			}
		}
	}
	
	public void clickDisable(){
		clickable = false;
	}
}