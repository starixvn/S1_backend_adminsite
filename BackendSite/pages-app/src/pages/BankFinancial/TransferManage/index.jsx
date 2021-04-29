import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import Edit from "./Edit";

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

	const [stateFilter, setFilter] = useState({
		TransferType: 0,
		FromDate: moment().add(-10, "days").format("l"),
		ToDate: moment().format("l"),
		Reason: 0,
		FromAccountNo: "",
		FromAccountDisplayName: "",
		ToAccountNo: "",
		ToAccountDisplayName: ""
	  });

	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let param = {
			Url: "TransferManage/GetTransferAccountReason"
		};
		WebAPI.PostData(param).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SET_REASON_LIST,
							reasonList: res.data.data
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

	const Filter_OnChange = (v, dataColumn) => {
		console.log("v",v);
		console.log("dataColumn",dataColumn);
		setFilter({
		  ...stateFilter,
		  [dataColumn]: v,
		});
	  };

	const Filter_Submit = () => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			console.log(stateFilter);
			let param = {
				Url: "TransferManage/GetTransferInfoList",
				Data: {
					TransferType: Number(stateFilter.TransferType),
                    TransFromDate: moment(stateFilter.FromDate).format('YYYY-MM-DD'),
                    TransToDate: moment(stateFilter.ToDate).format('YYYY-MM-DD'),
				    Reason:  Number(stateFilter.Reason),
                    FromAccountNo: stateFilter.FromAccountNo,
				    FromAccountDisplayName: stateFilter.FromAccountDisplayName,
				    ToAccountNo: stateFilter.ToAccountNo,
				    ToAccountDIsplayName: stateFilter.ToAccountDIsplayName,
				}
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {

							dispatch({
								type: defActionType.SET_TRANSFER_INFO_LIST,
								transferInfoList: res.data.data,
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

	const ClickCreateTransferInfo = async siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			if(siteId===null || siteId==="")
			{
				siteId=0;
			}
			let transferInfoData = {};
			try {
				const res = await WebAPI.PostData({
					Url: "TransferManage/CreateTransferInfo"
				})

				transferInfoData = {
					errorCode: res.data.errorCode,
					data: res.data.data,
					msg: res.data.message
				}

				if (transferInfoData.errorCode === 0) {
					if (isMountedRef.current) {
						setProcessing(false);
						dispatch({
							type: defActionType.GOTO_ADD_TRANSFER_INFO,
							transferInfoData: transferInfoData.data,
						});
					}

				} else {
					if (isMountedRef.current) {
						setProcessing(false);
					}
					let msg =
					transferInfoData.errorCode === 991 ? "API Server Error." : transferInfoData.msg;
					alert(msg);
				}
			} 
			catch (error) {
				if (isMountedRef.current) {
					setProcessing(false);
				}
				transferInfoData = { errorCode: 991 }
			}

		}
	};

	const ClickExportTransferInfo = async siteId => {
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

	const SetTransferInfo = info => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			console.log(info);
			let _info = {
				TransferId:info.transferId,
				TransferType:Number(info.transferType),
				TransactionType:Number(info.transactionType),
				TransDate:info.transDate,

				FromChannel: Number(info.fromChannel),
				FromAgent:info.fromAgent,
				FromPayId: Number(info.fromPayId),
				FromSiteId:Number(info.fromSiteId),
				FromSiteName:info.fromSiteName,
				FromCustId: Number(info.fromCustId),
				FromCurrencyId: Number(info.fromCurrencyId),
				FromSysId: Number(info.fromSysId),
				FromAccountNo: info.fromAccountNo,
				FromAccountDisplayName: info.fromAccountDisplayName,
				FromAmount: Number(info.fromAmount),
				FromFee: Number(info.fromFee),

				ToChannel: Number(info.toChannel),
				ToAgent:info.toAgent,
				ToPayId: Number(info.toPayId),
				ToSiteId:Number(info.toSiteId),
				ToSiteName:info.toSiteName,
				ToCustId: Number(info.toCustId),
				ToCurrencyId: Number(info.toCurrencyId),
				ToSysId: Number(info.toSysId),
				ToAccountNo: info.toAccountNo,
				ToAccountDisplayName: info.toAccountDisplayName,
				ToAmount: Number(info.toAmount),
				ToFee: Number(info.toFee),

				Reason: Number(info.reason),
				Remark: info.remark === undefined ? null : info.remark,
				CreateTime:info.createTime,
				Creator: Number(info.creator),
				UpdateTime:info.updateTime,
				Updator: Number(info.updator)
			};

			let param = {
				Url: "TransferManage/SetTransferInfo",
				Data:_info
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode <= 2 ) {
							dispatch({ type: defActionType.SET_TRANSFER_INFO });
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
	};

	const ClickEdit = async idx => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let transferInfoData = {};
			try {
				const res = await WebAPI.PostData({
					Url: "TransferManage/GetTransferInfoData",
					Data: {
						TransferId: state.transferInfoList[idx].transferId
					}
				})

				transferInfoData = {
					errorCode: res.data.errorCode,
					data: res.data.data,
					msg: res.data.message
				}

				if (transferInfoData.errorCode === 0) {
					if (isMountedRef.current) {
						setProcessing(false);
						dispatch({
							type: defActionType.EDIT_TRANSFER_INFO,
							transferInfoData: transferInfoData.data,
						});
					}

				} else {
					if (isMountedRef.current) {
						setProcessing(false);
					}
					let msg =
					transferInfoData.errorCode === 991 ? "API Server Error." : transferInfoData.msg;
					alert(msg);
				}
			} 
			catch (error) {
				if (isMountedRef.current) {
					setProcessing(false);
				}
				transferInfoData = { errorCode: 991 }
			}

		}
	};

	const ClickDelete = async idx => {
		if (
			window.confirm(
				`Delete Transfer which TransferId is ${state.transferInfoList[idx].transferId} ?`
			)
		) {
			let param = {
				Url: "TransferManage/DeleteTransferInfoData",
				Data: {
					TransferId: state.transferInfoList[idx].transferId
				}
			};
			WebAPI.PostData(param).then(res => {
				if (res.data.errorCode === 0) {
					Filter_Submit();
				}
			});
		}
	};

	const ClickBack = () => {
		dispatch({ type: defActionType.CLICK_BACK });
	};
	
	return state.panelType === defPanelType.Main ? (
		<Main
			filterInfo={stateFilter}
			ReasonList={state.reasonList}
			onChange={Filter_OnChange}
			Submit={Filter_Submit}

			ClickCreateTransferInfo={ClickCreateTransferInfo}
			ClickExportTransferInfo={ClickExportTransferInfo}
			ClickEdit={ClickEdit}
			ClickDelete={ClickDelete}
			transferInfoList={state.transferInfoList}
		/>
	) : (
			<Edit
				Datas={state.transferInfoData}
				SetFromCurrencyList={state.setFromCurrencyList}
				SetToCurrencyList={state.setToCurrencyList}
				action={{ SetTransferInfo, ClickBack, setProcessing }}
				IsAdd={state.panelType === defPanelType.Add}
				processing={processing}
			/>
		);
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
