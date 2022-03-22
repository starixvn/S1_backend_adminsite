import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import Edit from "./Edit";
import qs from "qs";
import {
	defActionType,
	defPanelType,
	reducer,
	initState
} from "./index.reducer";
import Main from "./index.main";

const App = () => {

	const [state, dispatch] = useReducer(reducer, initState);
	const [processing, setProcessing] = useState(false);

	const isMountedRef = useRef(null);

	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let param = {
			Url: "Common/AdminUserSiteList"
		};
		WebAPI.PostData(param).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SET_SITE_LIST,
							siteList: res.data.data
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
					setProcessing(false);
				}
			},
			err => {
				if (isMountedRef.current) {
					alert("API Server Error.");
					setProcessing(false);
				}
			}
		);
	}, []);

	const GetSiteTransferAccountList = siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "TransferAccountManage/GetTransferAccountInfoList",
				Data: {
					SiteId: siteId
				}
			};
			let site = state.siteList.find(s => s.siteId === siteId);
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {

							dispatch({
								type: defActionType.SET_TRANSFER_ACCOUNT_LIST,
								transferAccountList: res.data.data,
								setSite: siteId,
								setCurrencyList: site.iCurrencyList
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}

	const ClickCreateTransferAccount = async siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			if(siteId===null || siteId==="")
			{
				siteId=0;
			}
			let transferAccountData = {};
			try {
				const res = await WebAPI.PostData({
					Url: "TransferAccountManage/CreateTransferAccount",
					Data: {
						SiteId: siteId,
						CurrencyId: 0
					}
				})

				transferAccountData = {
					errorCode: res.data.errorCode,
					data: res.data.data,
					msg: res.data.message
				}

				if (transferAccountData.errorCode === 0) {
					if (isMountedRef.current) {
						setProcessing(false);
						dispatch({
							type: defActionType.GOTO_ADD_TRANSFER_ACCOUNT,
							transferAccountData: transferAccountData.data,
						});
					}

				} else {
					if (isMountedRef.current) {
						setProcessing(false);
					}
					let msg =
					transferAccountData.errorCode === 991 ? "API Server Error." : transferAccountData.msg;
					alert(msg);
				}
			} 
			catch (error) {
				if (isMountedRef.current) {
					setProcessing(false);
				}
				transferAccountData = { errorCode: 991 }
			}

		}
	};

	const ClickExportTransferAccount = async siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: 'TransferAccountManage/ExportTransferAccountInfoList',
				Data: {
					SiteId: siteId
				}
			};
	
			WebAPI.DownloadData(param);
	
			setTimeout(() => {
				setProcessing(false);
			}, 5000);

		}
	};

	const SetTransferAccount = account => {
		if (!processing) {
			if(!window.confirm("Update Account will Update Tranfer Data too. Aure you sure ?"))
			{
				setProcessing(false);
			}
			else
			{
				if (isMountedRef.current) {
					setProcessing(true);
				}
				let a = account.purpose.join();
				let _account = {
					Id:account.id,
					SiteId: Number(account.siteId),
					SiteName:account.siteName,
					CurrencyId: Number(account.currencyId),
					SysId: Number(account.sysId),
					Purpose: a,
					Channel: Number(account.channel),
					Agent: account.agent,
					PayId: Number(account.payId),
					AccountNo: account.accountNo,
					AccountName: account.accountName,
					DisplayName: account.displayName,
					InitialAmount: Number(account.initialAmount),
					MaxBalanceAlertAmount: Number(account.maxBalanceAlertAmount),
					Reason: Number(account.reason),
					Remark: account.remark === undefined ? null : account.remark,
					Status: Number(account.status),
					CreateTime:account.createTime,
					Creator: Number(account.creator),
					UpdateTime:account.updateTime,
					Updator: Number(account.updator)
				};

				let param = {
					Url: "TransferAccountManage/SetTransferAccount",
					Data:_account
				};
				WebAPI.PostData(param).then(
					res => {
						if (isMountedRef.current) {
							if (res.data.errorCode <= 2 ) {
								dispatch({ type: defActionType.SET_TRANSFER_ACCOUNT });
								GetSiteTransferAccountList(_account.SiteId);
							} 
							else {
								alert(res.data.message ?? "failed to set data.");
							}
							setProcessing(false);
						}
					},
					err => {
						if (isMountedRef.current) {
							alert("API Server Error.");
							setProcessing(false);
						}
					}
				);
			}
		}
	};

	const ClickEdit = async idx => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let transferAccountData = {};
			try {
				const res = await WebAPI.PostData({
					Url: "TransferAccountManage/GetTransferAccountData",
					Data: {
						Id: state.transferAccountList[idx].id,
						SiteId: state.transferAccountList[idx].siteId
					}
				})

				transferAccountData = {
					errorCode: res.data.errorCode,
					data: res.data.data,
					msg: res.data.message
				}

				if (transferAccountData.errorCode === 0) {
					if (isMountedRef.current) {
						setProcessing(false);
						dispatch({
							type: defActionType.EDIT_TRANSFER_ACCOUNT,
							transferAccountData: transferAccountData.data,
						});
					}

				} else {
					if (isMountedRef.current) {
						setProcessing(false);
					}
					let msg =
					transferAccountData.errorCode === 991 ? "API Server Error." : transferAccountData.msg;
					alert(msg);
				}
			} 
			catch (error) {
				if (isMountedRef.current) {
					setProcessing(false);
				}
				transferAccountData = { errorCode: 991 }
			}

		}
	};

	const ClickBack = () => {
		dispatch({ type: defActionType.CLICK_BACK });
	};
	
	return state.panelType === defPanelType.Main ? (
		<Main
		    GetSiteTransferAccountList={GetSiteTransferAccountList}
			ClickCreateTransferAccount={ClickCreateTransferAccount}
			ClickExportTransferAccount={ClickExportTransferAccount}
			ClickEdit={ClickEdit}
			siteList={state.siteList}
			setSite={state.setSite}
			transferAccountList={state.transferAccountList}
		/>
	) : (
			<Edit
				Datas={state.transferAccountData}
				SetSite={state.setSite}
				SetCurrencyList={state.setCurrencyList}
				action={{ SetTransferAccount, ClickBack, setProcessing }}
				IsAdd={state.panelType === defPanelType.Add}
				processing={processing}
			/>
		);
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
