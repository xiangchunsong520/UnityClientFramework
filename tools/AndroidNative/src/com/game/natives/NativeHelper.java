package com.game.natives;

import java.io.File;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.Intent;
import android.graphics.Point;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.Uri;
import android.text.ClipboardManager;
import android.util.Log;
import android.view.WindowManager;

public class NativeHelper
{
	public static int GetNetworkType(final Activity activity) 
	{
		try 
		{
			int netType = 0;
	        ConnectivityManager connectivityManager = (ConnectivityManager) activity.getSystemService(Context.CONNECTIVITY_SERVICE);
	        NetworkInfo networkInfo = connectivityManager.getActiveNetworkInfo();
	        if (networkInfo == null) {
	            return 0;
	        }        
	        int nType = networkInfo.getType();
	        if (nType == ConnectivityManager.TYPE_MOBILE) {
	        	netType = 2;
	        } else if (nType == ConnectivityManager.TYPE_WIFI) {
	            netType = 1;
	        }
	        return netType;
	    }
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:GetNetworkType", e.toString());
    	}
		return 0;
	}
	
    // 向剪贴板中添加文本
    //@SuppressWarnings("deprecation")
	public static void CopyTextToClipboard(final Activity activity, final String str) 
	{
    	try 
    	{
    		activity.runOnUiThread(new Runnable() 
    		{
    			@SuppressWarnings("deprecation")
				@Override
    			public void run(){
		    	ClipboardManager clipboard = (ClipboardManager) activity.getSystemService(Activity.CLIPBOARD_SERVICE);
		        clipboard.setText(str);
		        //Toast.makeText(activity, str,Toast.LENGTH_LONG ).show();
		        }
    		});
    	}
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:CopyTextToClipboard", e.toString());
    	}
    }    
	
    // 从剪贴板中获取文本
    @SuppressWarnings("deprecation")
	public static String GetTextFromClipboard(final Activity activity) 
    {
    	try 
    	{
    		//Context activity = com.unity3d.player.UnityPlayer.currentActivity;
	    	ClipboardManager clipboard = (ClipboardManager) activity.getSystemService(Activity.CLIPBOARD_SERVICE);
	        return clipboard.getText().toString();
    	}
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:GetTextFromClipboard", e.toString());
    	}
        return "";
    }

	static ProgressDialog MyDialog;
	public static void ShowLoading(final Activity activity, final String str) 
	{
		try 
		{
    		activity.runOnUiThread(new Runnable() 
    		{
    			@Override
    			public void run(){
					if (MyDialog == null)
					{
						MyDialog = new ProgressDialog(activity,ProgressDialog.THEME_HOLO_DARK);
						MyDialog.setProgressStyle(ProgressDialog.STYLE_SPINNER);  
						//MyDialog.setProgressStyle(ProgressDialog.THEME_DEVICE_DEFAULT_DARK);
					}
					//MyDialog.setTitle(title);
					//MyDialog.setMessage(message);
					//MyDialog.setIndeterminate(indeterminate);
					//MyDialog.setCancelable(cancelable);
					//MyDialog.setOnCancelListener(cancelListener);
					MyDialog.setCancelable(false);  
					MyDialog.setMessage(str);
					MyDialog.show();
					Point size = new Point();
					MyDialog.getWindow().getWindowManager().getDefaultDisplay().getSize(size);
					//int height = size.y;
					int width = size.x;
					WindowManager.LayoutParams params = MyDialog.getWindow().getAttributes();
					//params.alpha = 0.8f;
					//params.gravity = Gravity.CENTER;
					//params.height = height;
					params.width = width * 3 / 5;
					//params.dimAmount = 0f;
					MyDialog.getWindow().setAttributes(params);
		        }
    		});
    	}
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:ShowLoading", e.toString());
    	}
    }    
    
    public static void HideLoading(final Activity activity) 
    {
		try 
		{
    		activity.runOnUiThread(new Runnable() 
    		{
    			@Override
    			public void run(){
    				if (MyDialog != null){
    					MyDialog.setMessage("");
						MyDialog.setCancelable(true);  
    					MyDialog.hide();
    				}
		        }
    		});
    	}
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:HideLoading", e.toString());
    	}
    }
    
    public static void InstallApk(final Activity activity, final String fileName)
	{
		try 
		{
			activity.runOnUiThread(new Runnable() 
			{
				@Override
				public void run(){
					Intent intent = new Intent();
			        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);  
			        intent.setAction(Intent.ACTION_VIEW);  
					intent.setDataAndType(Uri.fromFile(new File(fileName)), "application/vnd.android.package-archive");
					activity.startActivity(intent);
				}
			});
		}
    	catch(Exception e)
    	{
    		Log.e("NativeHelper:SetupApk", e.toString());
    	}
	}
}
