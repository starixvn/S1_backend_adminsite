import moment from "moment";
export const defActionType = {
	SET_SITE: "SET_SITE",
	SET_VALUE_BY_LANG: "SET_VALUE_BY_LANG",
	SET_VALUE: "SET_VALUE"
};

export const initialState = (IsAdd, datas, siteList) => {
	if (IsAdd) {
		return {
			siteId: null,
			langList: [],
			title: {},
			content: {},
			imageURL: {},
			postTime: moment(),
			sort: 0,
			status: 0
		};
	} else {
		let langList = siteList?.find(x => x.siteId === datas.siteId)?.langList;

		return {
			siteId: datas.siteId,
			langList: langList ?? [],
			title: JSON.parse(datas.title),
			content: JSON.parse(datas.content),
			imageURL: JSON.parse(datas.imageURL),
			postTime: datas.postTime,
			sort: datas.sort,
			status: datas.status,
			newsId: datas.newsId
		};
	}
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.SET_SITE:
			let title = {};
			let content = {};
			let imageURL = {};
			action.langList.forEach(l => {
				title[l] = "";
				content[l] = "";
				imageURL[l] = "";
			});
			return {
				...state,
				siteId: action.siteId,
				langList: action.langList,
				title: title,
				content: content,
				imageURL: imageURL
			};

		case defActionType.SET_VALUE_BY_LANG:
			let result = { ...state[action.col] };
			result[action.langId] = action.value;
			return {
				...state,
				[action.col]: { ...result }
			};

		case defActionType.SET_VALUE:
			return {
				...state,
				[action.col]: action.value
			};

		default:
			throw new Error();
	}
}
