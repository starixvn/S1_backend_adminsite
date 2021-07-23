export const dateFormat = (createTime)=>{
    return moment.utc(createTime).add(8,'hour').format('l LTS');
}

export const boolFormat = (v)=>{
    return v ? 'true' : 'false';
}

export const columnDefs = [
    {
        dataColumn:'createTime', 
        columnTitle:`CreateTime`,
        body:{
            format:dateFormat
        }
    },
    {
        dataColumn:'flag', 
        columnTitle:'Read',
        body:{
            format:boolFormat
        }
    },
    { dataColumn:'title',  columnTitle:'Subject' },
    { dataColumn:'content',  columnTitle:'Content' },
    { dataColumn:'sender',  columnTitle:'Sender' }
];