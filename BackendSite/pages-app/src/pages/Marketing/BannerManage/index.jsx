import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
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

	const GetSiteBannerList = siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "BannerManage/GetBannerList",
				Data: qs.stringify({
					SiteId: siteId
				})
			};
			let site = state.siteList.find(s => s.siteId === siteId);
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_BANNER_LIST,
								bannerList: res.data.data,
								setSite: siteId,
								setCurrencyList: site.iCurrencyList,
								setLangList: site.iLangList
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

	const ClickCreateBanner = () => {
		dispatch({ type: defActionType.GOTO_ADD_BANNER, });
	};

	const SetBanner = banner => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let _specific = Object.keys(banner.specific)
				.filter(c => {
					return banner.selectedCurrencies.includes(Number(c));
				})
				.map(s => {
					let info = banner.specific[s];
					let lang = {};
					info.selectedLang.forEach(l => {
						lang[l] = info.lang[l];
					});
					return {
						CurrencyId: s,
						Details: JSON.stringify(lang),
						Status: info.status
					};
				});

			let _banner = {
				SiteId: Number(banner.siteId),
				BannerName: banner.bannerName,
				BannerType: Number(banner.bannerType),
				Sort: Number(banner.sort),
				ActionMode: Number(banner.actionMode),
				ShowIndex: Number(banner.showIndex),
				BonusCode: banner.bonusCode,
				TpId: banner.tpId,
				GameId: banner.gameId,
				Specific: _specific
			};
			let param = {
				Url: "BannerManage/SetBanner",
				Data: qs.stringify({
					// IsAdd: state.panelType === defPanelType.Add,
					bannerInfo: _banner
				})
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({ type: defActionType.SET_BANNER });
							GetSiteBannerList(_banner.SiteId);
						} else if (res.data.errorCode === 1062) {
							alert("Duplicate Bonus Code.");
						} else {
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
			const bannerData = await WebAPI.PostData({
				Url: "BannerManage/GetBanner",
				Data: qs.stringify({
					SiteId: state.bannerList[idx].siteId,
					BannerName: state.bannerList[idx].bannerName
				})
			}).then(
				res => {
					return {
						errorCode: res.data.errorCode,
						data: res.data.data,
						msg: res.data.message
					};
				},
				err => {
					return { errorCode: 991 };
				}
			);

			if (bannerData.errorCode === 0) {
				dispatch({
					type: defActionType.EDIT_BANNER,
					bannerData: bannerData.data,
				});
			} else {
				let msg =
					bannerData.errorCode === 991 ? "API Server Error." : bannerData.msg;
				alert(msg);
			}

			if (isMountedRef.current) {
				setProcessing(false);
			}
		}
	};

	const UpdateBannerStatus = idx => {
		let _banner = state.bannerList[idx];
		let msg =
			_banner.status === 1
				? "Disable this banner all currency?"
				: "Enable this banner all currency?";
		if (_banner && window.confirm(msg)) {
			let newStatus = _banner.status === 0 ? 1 : 0;
			let param = {
				Url: "BannerManage/UpdateBannerStatus",
				Data: qs.stringify({
					SiteId: _banner.siteId,
					BannerName: _banner.bannerName,
					Status: newStatus
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						GetSiteBannerList(_banner.siteId);
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			});
		}
	};

	const ClickBack = () => {
		dispatch({ type: defActionType.CLICK_BACK });
	};

	const skipField = {
		"default": ["bonusCode", "matchId", "tpId", "gameId"]
	}
	return state.panelType === defPanelType.Main ? (
		<Main
			GetSiteBannerList={GetSiteBannerList}
			ClickCreateBanner={ClickCreateBanner}
			ClickEdit={ClickEdit}
			UpdateBannerStatus={UpdateBannerStatus}
			siteList={state.siteList}
			setSite={state.setSite}
			bannerList={state.bannerList}
		/>
	) : (
			<Edit
				Datas={state.bannerData}
				SetSite={state.setSite}
				SetCurrencyList={state.setCurrencyList}
				SetLangList={state.setLangList}
				action={{ SetBanner, ClickBack }}
				IsAdd={state.panelType === defPanelType.Add}
				processing={processing}
				skipField={skipField}
			/>
		);
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
