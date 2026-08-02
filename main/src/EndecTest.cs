using io.wispforest.endec.impl;

namespace io.wispforest.endec;

internal class EndecTest {
    public record ItemStatePacket(int? ItemId, float Health, byte StateFlags)  {
        public static readonly StructEndec<ItemStatePacket> ENDEC = StructEndecBuilder.of<ItemStatePacket>(ctx => 
            ctx.of(
    			ctx.nullable(Endec.INT, "ItemId", value => value.ItemId),
    			(Endec.FLOAT, "Health", value => value.Health),
    			(Endec.BYTE, "StateFlags", value => value.StateFlags),
    		    (int? ItemId, float Health, byte StateFlags) => new ItemStatePacket(ItemId, Health, StateFlags)
            )
        );
    }
}